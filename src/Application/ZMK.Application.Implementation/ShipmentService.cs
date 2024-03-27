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

public class ShipmentService : BaseService, IShipmentService
{
    public ShipmentService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        ZMKDbContext dbContext, 
        ICurrentSessionProvider currentSessionProvider, 
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> AddAsync(ShipmentAddDTO dTO, CancellationToken cancellationToken = default)
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

        _logger.LogInformation("Попытка добавления отгрузки для проекта {projectId}.", dTO.ProjectId);
        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        var shipment = new Shipment
        {
            ProjectId = dTO.ProjectId,
            CreatedDate = currentDate,
            CreatorId = isAbleResult.Value.UserId,
            Number = dTO.Number.Trim(),
            Remark = dTO.Remark?.Trim(),
            ShipmentDate = new DateTimeOffset(dTO.ShipmentDate.Year, dTO.ShipmentDate.Month, dTO.ShipmentDate.Day, 0, 0, 0, TimeSpan.Zero),
        };

        if (await _dbContext.Shipments.AnyAsync(e => e.ProjectId == shipment.ProjectId && e.Number == shipment.Number, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Отгрузка с таким номером уже существует."));
        }

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Отгрузка успешно добавлена.");
        return shipment.Id;
    }

    public async Task<Result> UpdateAsync(ShipmentUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(cancellationToken, DefaultRoles.Admin, DefaultRoles.User).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        var targetShipment = await _dbContext.Shipments.SingleOrDefaultAsync(e => e.Id == dTO.ShipmentId, cancellationToken);
        if (targetShipment is not null)
        {
            targetShipment.Number = dTO.Number.Trim();
            targetShipment.ShipmentDate = new DateTimeOffset(dTO.ShipmentDate.Year, dTO.ShipmentDate.Month, dTO.ShipmentDate.Day, 0, 0, 0, TimeSpan.Zero);
            targetShipment.Remark = dTO.Remark?.Trim();
        }

        _logger.LogInformation("Попытка обновления данных о погрузке.");
        switch (targetShipment)
        {
            case null:
                return Result.Failure(Errors.NotFound("Погрузка"));
            case Shipment when await _dbContext.Shipments.Where(e => e.ProjectId == targetShipment.ProjectId).AnyAsync(e => e.Id != targetShipment.Id && targetShipment.Number == e.Number):
                return Result.Failure(new Error(nameof(Error), $"Погрузка с таким номером '{targetShipment.Number}' уже существует."));
            default:
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Данные о погрузке успешно обновлены.");
                    return Result.Success();
                }
        }
    }
}