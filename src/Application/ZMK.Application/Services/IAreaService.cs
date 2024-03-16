using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IAreaService
{
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> AddAsync(AreaAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(AreaUpdateDTO dTO, CancellationToken cancellationToken = default);
}