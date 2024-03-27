using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IShipmentService
{
    Task<Result<Guid>> AddAsync(ShipmentAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(ShipmentUpdateDTO dTO, CancellationToken cancellationToken = default);
}