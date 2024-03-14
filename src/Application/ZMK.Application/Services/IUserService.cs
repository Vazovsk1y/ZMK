using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IUserService
{
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> AddAsync(UserAddDTO dTO, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(UserUpdateDTO dTO, CancellationToken cancellationToken = default);
}