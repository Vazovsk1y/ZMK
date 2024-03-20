using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.Domain.Common;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class MarkService : BaseService, IMarkService
{
    public static readonly IReadOnlyCollection<string> AllowedFilesExtensions = [ ".xlsx", ".xls" ];

    private readonly IXlsxReader<MarkAddDTO> _xlsxMarksReader;
    public MarkService(IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        ICurrentSessionProvider currentSessionProvider,
        UserManager<User> userManager,
        IXlsxReader<MarkAddDTO> xlsxMarksReader) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
        _xlsxMarksReader = xlsxMarksReader;
    }

    public async Task<Result> FillExecutionAsync(FillExecutionDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(cancellationToken, DefaultRoles.Admin, DefaultRoles.User).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка заполненения выполнения марки.");
        var mark = await _dbContext
            .Marks
            .Include(e => e.Project)
            .ThenInclude(e => e.Settings)
            .Include(e => e.Project.Areas)
            .SingleOrDefaultAsync(e => e.Id == dTO.MarkId, cancellationToken);

        switch (mark)
        {
            case null:
                return Result.Failure(Errors.NotFound("Марка"));
            case Mark when mark.Project.Settings.AreExecutorsRequired && dTO.AreasExecutions.Select(e => e.Executors).Any(e => !e.Any()):
                return Result.Failure(new Error(nameof(Error), "Заполнение исполнителей обязательно. Указано в настройках проэкта."));
            case Mark when dTO.AreasExecutions.Select(e => e.AreaId).Any(i => !mark.Project.Areas.Select(e => e.AreaId).Contains(i)):
                throw new InvalidOperationException($"Один из переданных участков не определен для проэкта с id '{mark.Project.Id}'.");
            default:
                {
                    var completeEvents = new List<MarkCompleteEvent>();
                    var completeEventsEmployees = new List<MarkCompleteEventEmployee>();

                    foreach (var item in dTO.AreasExecutions)
                    {
                        double completeCount = await _dbContext
                            .MarkCompleteEvents
                            .Where(e => e.MarkId == dTO.MarkId && e.AreaId == item.AreaId)
                            .SumAsync(e => e.CompleteCount, cancellationToken);

                        double leftCount = mark.Count - completeCount;
                        if (item.Count > leftCount)
                        {
                            return Result.Failure(new Error(nameof(Error), $"Количество для заполения '{item.Count}' больше текущего остатка на этом участке '{leftCount}'."));
                        }

                        var currentDate = _clock.GetDateTimeOffsetUtcNow();
                        var @event = new MarkCompleteEvent
                        {
                            MarkId = mark.Id,
                            AreaId = item.AreaId,
                            CompleteCount = item.Count,
                            CreatedDate = new DateTimeOffset(item.Date.Year, item.Date.Month, item.Date.Day, currentDate.Hour, currentDate.Minute, currentDate.Second, TimeSpan.Zero), // UTC
                            CreatorId = isAbleResult.Value.UserId,
                            EventType = EventType.Complete,
                            Remark = item.Remark?.Trim(),
                            MarkCode = mark.Code,
                            MarkCount = mark.Count,
                            MarkOrder = mark.Order,
                            MarkTitle = mark.Title,
                            MarkWeight = mark.Weight,
                        };
                        completeEvents.Add(@event);
                        completeEventsEmployees.AddRange(item.Executors.Select(e => new MarkCompleteEventEmployee { EmployeeId = e, EventId = @event.Id }));
                    }

                    _dbContext.MarkCompleteEvents.AddRange(completeEvents);
                    _dbContext.MarkCompleteEventsEmployees.AddRange(completeEventsEmployees);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Выполнение марки было успешно заполнено.");
                    return Result.Success();
                }
        }
    }

    public async Task<Result<IReadOnlyCollection<Guid>>> AddFromXlsxAsync(string filePath, Guid projectId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!AllowedFilesExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(new Error(nameof(Error), "Некорректное разрешение файла."));
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(isAbleResult.Errors);
        }

        var isAbleToAddMark = await IsAbleToAddMark(projectId);
        if (isAbleToAddMark.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(isAbleToAddMark.Errors);
        }

        _logger.LogInformation("Попытка добавления марок из файла таблицы эксель {fileExtension}.", Path.GetExtension(filePath));
        List<(Mark mark, string? remark)> marks = [];
        try
        {
            var marksDTos = _xlsxMarksReader.Read(filePath, projectId);
            foreach (var markDto in marksDTos)
            {
                var validationResult = Validate(markDto);
                if (validationResult.IsFailure)
                {
                    return Result.Failure<IReadOnlyCollection<Guid>>(validationResult.Errors);
                }
                marks.Add((markDto.ToEntity(), markDto.Remark?.Trim()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Что-то пошло не так во время чтения марок из файла.");
            return Result.Failure<IReadOnlyCollection<Guid>>(new Error(nameof(Exception), ex.Message));
        }

        bool isAllMarksUnique = !await _dbContext
            .Marks
            .AsNoTracking()
            .Where(e => e.ProjectId == projectId)
            .Select(e => e.Code)
            .AnyAsync(e => marks.Select(e => e.mark.Code).Contains(e), cancellationToken);

        if (!isAllMarksUnique)
        {
            return Result.Failure<IReadOnlyCollection<Guid>>(new Error(nameof(Error), "Проверьте коды марок из файла, некоторые уже были добавлены ранее."));
        }

        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        var addEvents = marks.Select(e => new MarkEvent
        {
            MarkCount = e.mark.Count,
            MarkCode = e.mark.Code,
            MarkOrder = e.mark.Order,
            MarkTitle = e.mark.Title,
            MarkWeight = e.mark.Weight,
            CreatedDate = currentDate,
            CreatorId = isAbleResult.Value.UserId,
            MarkId = e.mark.Id,
            EventType = EventType.Create,
            Remark = e.remark,
        });

        _dbContext.MarksEvents.AddRange(addEvents);
        _dbContext.Marks.AddRange(marks.Select(e => e.mark));
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Марки были успешно добавлены.");
        return marks.Select(e => e.mark.Id).ToList();
    }

    public async Task<Result<Guid>> AddAsync(MarkAddDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        var isAbleToAddMark = await IsAbleToAddMark(dTO.ProjectId);
        if (isAbleToAddMark.IsFailure)
        {
            return Result.Failure<Guid>(isAbleToAddMark.Errors);
        }

        _logger.LogInformation("Попытка добавления новой марки.");
        var mark = dTO.ToEntity();
        if (await _dbContext.Marks.Where(e => e.ProjectId == mark.ProjectId).AnyAsync(e => e.Code == mark.Code, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Марка с таким кодом уже существует."));
        }

        var addEvent = new MarkEvent
        {
            MarkCount = mark.Count,
            MarkCode = mark.Code,
            MarkOrder = mark.Order,
            MarkTitle = mark.Title, 
            MarkWeight = mark.Weight,
            CreatedDate = _clock.GetDateTimeOffsetUtcNow(),
            CreatorId = isAbleResult.Value.UserId,
            EventType = EventType.Create,
            MarkId = mark.Id,
            Remark = dTO.Remark,
        };

        _dbContext.Marks.Add(mark);
        _dbContext.MarksEvents.Add(addEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Марка была успешно добавлена.");
        return mark.Id;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        var mark = await _dbContext
            .Marks
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        _logger.LogInformation("Попытка удаления марки.");
        if (mark is null)
        {
            _logger.LogWarning("Марки с указанным айди нет в базе данных.");
            return Result.Success();
        }

        var isAbleToDeleteMark = await IsAbleToDeleteMark(mark.ProjectId);
        if (isAbleToDeleteMark.IsFailure)
        {
            return Result.Failure<Guid>(isAbleToDeleteMark.Errors);
        }

        _dbContext.Marks.Remove(mark);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Марка была успешно удалена.");
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(MarkUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка обновления информации о марке.");
        var mark = await _dbContext
            .Marks
            .SingleOrDefaultAsync(e => e.Id == dTO.Id, cancellationToken);

        mark?.Update(dTO);
        switch (mark)
        {
            case null:
                return Result.Failure(Errors.NotFound("Марка"));
            case Mark when await IsAbleToModifyMark(mark.ProjectId) is Result result && result.IsFailure:
                return result;
            case Mark when await _dbContext.Marks.Where(e => e.ProjectId == mark.ProjectId).AnyAsync(e => e.Id != mark.Id && e.Code == mark.Code, cancellationToken):
                return Result.Failure(new Error(nameof(Error), "Марка с таким кодом уже существует."));
            default:
                {
                    var completeForEachArea = await _dbContext
                        .MarkCompleteEvents
                        .AsNoTracking()
                        .Include(e => e.Area)
                        .Where(e => e.MarkId == mark.Id)
                        .GroupBy(e => e.Area)
                        .ToDictionaryAsync(e => e.Key, e => e.Sum(c => c.CompleteCount), cancellationToken);

                    if (completeForEachArea.FirstOrDefault(e => e.Value > mark.Count) is KeyValuePair<Area, double> value && value.Key is not null)
                    {
                        return Result.Failure(new Error(nameof(Error), $"Текущее кол-во '{value.Value}' выполненных марок для '{value.Key.Title}' больше чем то количество которое вы пытаетесь установить '{mark.Count}'."));
                    }

                    var updateEvent = new MarkEvent
                    {
                        CreatedDate = _clock.GetDateTimeOffsetUtcNow(),
                        MarkId = mark.Id,
                        MarkCount = mark.Count,
                        MarkCode = mark.Code,
                        MarkOrder = mark.Order,
                        MarkTitle = mark.Title,
                        MarkWeight = mark.Weight,
                        CreatorId = isAbleResult.Value.UserId,
                        EventType = EventType.Modify,
                        Remark = "Произведено изменение данных о марке.",
                    };

                    _dbContext.MarksEvents.Add(updateEvent);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Информация о марке была успешно обновлена.");
                    return Result.Success();
                }
        }
    }

    private async Task<Result> IsAbleToAddMark(Guid projectId)
    {
        var project = await _dbContext
            .Projects
            .Include(e => e.Settings)
            .SingleOrDefaultAsync(e => e.Id == projectId);

        if (project is null || !project.Settings.AllowMarksAdding)
        {
            return Result.Failure(new Error(nameof(Error), "Функция добавления марок отключена в настройках проэкта."));
        }

        return Result.Success();
    }

    private async Task<Result> IsAbleToDeleteMark(Guid projectId)
    {
        var project = await _dbContext
            .Projects
            .Include(e => e.Settings)
            .SingleOrDefaultAsync(e => e.Id == projectId);

        if (project is null || !project.Settings.AllowMarksDeleting)
        {
            return Result.Failure(new Error(nameof(Error), "Функция удаления марок отключена в настройках проэкта."));
        }

        return Result.Success();
    }

    private async Task<Result> IsAbleToModifyMark(Guid projectId)
    {
        var project = await _dbContext
            .Projects
            .Include(e => e.Settings)
            .SingleOrDefaultAsync(e => e.Id == projectId);

        if (project is null || !project.Settings.AllowMarksModifying)
        {
            return Result.Failure(new Error(nameof(Error), "Функция изменения марок отключена в настройках проэкта."));
        }

        return Result.Success();
    }
}
