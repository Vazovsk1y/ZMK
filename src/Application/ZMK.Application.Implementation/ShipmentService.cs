using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
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
}