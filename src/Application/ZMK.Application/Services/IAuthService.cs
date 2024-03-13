using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IAuthService
{
    Task<Result<SessionDTO>> LoginAsync(UserLoginDTO loginDTO, CancellationToken cancellationToken = default);

    Task<Result> LogoutAsync(CancellationToken cancellationToken = default);
}
