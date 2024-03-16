﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
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
    public static readonly IReadOnlyCollection<string> AllowedExtensions = [".xlsx", ".xls"];

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

    public async Task<Result<IReadOnlyCollection<Guid>>> AddFromXlsxAsync(string filePath, Guid projectId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!AllowedExtensions.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase))
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

        _logger.LogInformation("Попытка добавления марок из файла таблицы.");
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
            Count = e.mark.Count,
            CreatedDate = currentDate,
            CreatorId = isAbleResult.Value.UserId,
            MarkId = e.mark.Id,
            EventType = EventType.Create,
            Remark = e.remark,
        });

        _dbContext.MarksEvents.AddRange(addEvents);
        _dbContext.Marks.AddRange(marks.Select(e => e.mark));
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Все марки были успешно добавлены.");
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
            Count = mark.Count,
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

        _logger.LogInformation("Попытка обновления марки.");
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
                    var updateEvent = new MarkEvent
                    {
                        CreatedDate = _clock.GetDateTimeOffsetUtcNow(),
                        MarkId = mark.Id,
                        Count = mark.Count,
                        CreatorId = isAbleResult.Value.UserId,
                        EventType = EventType.Modify,
                        Remark = "Произведено изменение марки.",
                    };

                    _dbContext.MarksEvents.Add(updateEvent);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Марка была успешно обновлена.");
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
