using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IAuthService
{
    Task<Result<Guid>> LoginAsync(UserLoginDTO loginDTO, CancellationToken cancellationToken = default);

    Result Logout();
}
