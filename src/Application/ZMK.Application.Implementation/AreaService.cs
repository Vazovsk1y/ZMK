using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Services;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class AreaService : BaseService, IAreaService
{
    public AreaService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        ZMKDbContext dbContext, 
        ICurrentSessionProvider currentSessionProvider, 
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> AddAsync(AreaAddDTO dTO, CancellationToken cancellationToken = default)
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

        _logger.LogInformation("Попытка добавления нового участка.");
        var area = new Area
        {
            Order = dTO.Order,
            Remark = dTO.Remark,
            Title = dTO.Title.Trim(),
        };

        switch (area)
        {
            case Area when await _dbContext.Areas.AnyAsync(e => e.Title == area.Title, cancellationToken):
                return Result.Failure<Guid>(new Error(nameof(Error), "Участок с таким названием уже существует."));
            case Area when await _dbContext.Areas.AnyAsync(e => e.Order == area.Order, cancellationToken):
                return Result.Failure<Guid>(new Error(nameof(Error), "Участок с таким значением очередности уже существует."));
            default:
                {
                    _dbContext.Areas.Add(area);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Участок был успешно добавлен.");
                    return area.Id;
                }
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка удаления участка.");
        var area = await _dbContext
            .Areas
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (area is null)
        {
            _logger.LogWarning("Участка с указанным айди нет в базе данных.");
            return Result.Success();
        }

        if (await _dbContext.MarkCompleteEvents.AnyAsync(e => e.AreaId == area.Id, cancellationToken) || await _dbContext.ProjectsAreas.AnyAsync(e => e.AreaId == area.Id, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Удаление невозможно, присутствуют связанные данные."));
        }

        _dbContext.Areas.Remove(area);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Участок был успешно удален.");
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(AreaUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return isAbleResult;
        }

        _logger.LogInformation("Попытка обновления информации об участке.");
        var area = await _dbContext
            .Areas
            .SingleOrDefaultAsync(e => e.Id == dTO.AreaId, cancellationToken);

        if (area is not null)
        {
            area.Order = dTO.Order;
            area.Remark = dTO.Remark?.Trim();
            area.Title = dTO.Title.Trim();
        }

        switch (area)
        {
            case null:
                return Result.Failure(Errors.NotFound("Участок"));
            case Area when await _dbContext.Areas.AnyAsync(e => e.Id != area.Id && e.Title == area.Title, cancellationToken):
                return Result.Failure<Guid>(new Error(nameof(Error), $"Участок с таким названием '{area.Title}' уже существует."));
            case Area when await _dbContext.Areas.AnyAsync(e => e.Id != area.Id && e.Order == area.Order, cancellationToken):
                return Result.Failure<Guid>(new Error(nameof(Error), $"Участок с таким значением очередности '{area.Order}' уже существует."));
            default:
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Информация об участке была успешно обновлена.");
                    return Result.Success();
                }
        }
    }
}