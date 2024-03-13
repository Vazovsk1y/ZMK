using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class AuthService : BaseService, IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentSessionProvider _currentSessionProvider;
    public AuthService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        UserManager<User> userManager,
        ICurrentSessionProvider currentSessionProvider) : base(clock, logger, serviceScopeFactory, dbContext)
    {
        _userManager = userManager;
        _currentSessionProvider = currentSessionProvider;
    }

    public async Task<Result<SessionDTO>> LoginAsync(UserLoginDTO loginDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(loginDTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<SessionDTO>(validationResult.Errors);
        }

        var user = await _userManager.FindByNameAsync(loginDTO.Username);

        switch (user)
        {
            case null:
                return Result.Failure<SessionDTO>(Errors.Auth.InvalidUsernameOrPassword);
            case User when !await _userManager.CheckPasswordAsync(user, loginDTO.Password):
                return Result.Failure<SessionDTO>(Errors.Auth.InvalidUsernameOrPassword);
            case User when await _dbContext.Sessions.AnyAsync(e => e.IsActive, cancellationToken):
                return Result.Failure<SessionDTO>(Errors.Auth.SessionIsAlreadyOpened);
            default:
                {
                    return await AddOrUpdateSession(user, cancellationToken);
                }
        }
    }

    public async Task<Result> LogoutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentSessionId = _currentSessionProvider.GetCurrentSessionId();
        if (currentSessionId is null)
        {
            return Result.Success();
        }

        var session = await _dbContext.Sessions.SingleOrDefaultAsync(e => e.Id == currentSessionId, cancellationToken);

        switch (session)
        {
            case null:
                return Result.Success();
            case Session { IsActive: false }:
                return Result.Success();
            default:
                {
                    var currentDate = _clock.GetDateTimeOffsetUtcNow();
                    session.ClosingDate = currentDate;
                    session.IsActive = false;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return Result.Success();
                }
        }
    }

    private async Task<Result<SessionDTO>> AddOrUpdateSession(User user, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions.SingleOrDefaultAsync(e => e.UserId == user.Id, cancellationToken);

        var currentDate = _clock.GetDateTimeOffsetUtcNow();
        if (session is null)
        {
            session = new Session()
            {
                IsActive = true,
                UserId = user.Id,
                CreationDate = currentDate,
                ClosingDate = null,
            };

            _dbContext.Sessions.Add(session);
        }
        else
        {
            session.IsActive = true;
            session.CreationDate = currentDate;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(session.ToDTO());
    }
}
