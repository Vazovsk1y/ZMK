using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Services;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class AuthService : BaseService, IAuthService
{
    public AuthService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        UserManager<User> userManager,
        ICurrentSessionProvider currentSessionProvider) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> LoginAsync(UserLoginDTO loginDTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation("Попытка входа в аккаунт.");

        var validationResult = Validate(loginDTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var user = await _userManager.FindByNameAsync(loginDTO.UserName).ConfigureAwait(false);

        switch (user)
        {
            case null:
                return Result.Failure<Guid>(Errors.Auth.InvalidUsernameOrPassword);
            case User when await _userManager.HasPasswordAsync(user) && string.IsNullOrWhiteSpace(loginDTO.Password):
                return Result.Failure<Guid>(Errors.Auth.InvalidUsernameOrPassword);
            case User when !string.IsNullOrWhiteSpace(loginDTO.Password) && !await _userManager.CheckPasswordAsync(user, loginDTO.Password):
                return Result.Failure<Guid>(Errors.Auth.InvalidUsernameOrPassword);
            case User when await _dbContext.Sessions.AnyAsync(e => e.IsActive, cancellationToken):
                return Result.Failure<Guid>(Errors.Auth.SessionIsAlreadyOpened);
            default:
                {
                    await _dbContext
                        .Entry(user)
                        .Reference(e => e.Employee)
                        .LoadAsync(cancellationToken)
                        .ConfigureAwait(false);

                    return await AddOrUpdateSession(user, cancellationToken).ConfigureAwait(false);
                }
        }
    }

    public Result Logout()
    {
        _logger.LogInformation("Выход из аккаунта...");

        var currentSessionId = _currentSessionProvider.GetCurrentSessionId();
        if (currentSessionId is null)
        {
            return Result.Success();
        }

        var session = _dbContext.Sessions
            .Include(e => e.User)
            .Include(e => e.User!.Employee)
            .SingleOrDefault(e => e.Id == currentSessionId);

        switch (session)
        {
            case null:
                return Result.Success();
            default:
                {
                    var currentDate = _clock.GetDateTimeOffsetUtcNow();
                    session.ClosingDate = currentDate;
                    session.IsActive = false;

                    _dbContext.SaveChanges();
                    _logger.LogInformation("'{userName} - {employeeFullName}' успешно вышел из аккаунта.", session.User!.UserName, session.User.Employee!.FullName);
                    return Result.Success();
                }
        }
    }

    private async Task<Result<Guid>> AddOrUpdateSession(User user, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions.SingleOrDefaultAsync(e => e.UserId == user.Id, cancellationToken).ConfigureAwait(false);

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
            _logger.LogInformation("Создана новая сессия.");
        }
        else
        {
            session.IsActive = true;
            session.CreationDate = currentDate;
            session.ClosingDate = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Вы вошли в аккаунт '{userName} - {employeeName}'.", user.UserName, user.Employee!.FullName);
        return Result.Success(session.Id);
    }
}
