using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public abstract class BaseService
{
    protected readonly IClock _clock;
    protected readonly ILogger _logger;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly ZMKDbContext _dbContext;
    protected readonly ICurrentSessionProvider _currentSessionProvider;
    protected readonly UserManager<User> _userManager;

    protected BaseService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        ICurrentSessionProvider currentSessionProvider,
        UserManager<User> userManager)
    {
        _clock = clock;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _dbContext = dbContext;
        _currentSessionProvider = currentSessionProvider;
        _userManager = userManager;
    }

    protected Result Validate<T>(T @object) where T : notnull
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<IValidator<T>>();

        var validationResult = validator.Validate(@object);
        if (!validationResult.IsValid)
        {
            return Result.Failure(validationResult.Errors.Select(e => new Error(e.ErrorCode, e.ErrorMessage)));
        }

        return Result.Success();
    }

    protected Result Validate(params object[] objects)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        foreach (var item in objects)
        {
            var itemType = item.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(itemType);
            var validator = scope.ServiceProvider.GetRequiredService(validatorType);
            var validationResult = (ValidationResult)validator.GetType().GetMethod(nameof(IValidator.Validate), [itemType])!.Invoke(validator, new[] { item })!;
            if (!validationResult.IsValid)
            {
                return Result.Failure(validationResult.Errors.Select(e => new Error(e.ErrorCode, e.ErrorMessage)));
            }
        }

        return Result.Success();
    }

    protected async Task<Result<Session>> IsAbleToPerformAction(string role, CancellationToken cancellationToken = default)
    {
        var currentSession = await _dbContext.Sessions.LoadByIdAsync(_currentSessionProvider.GetCurrentSessionId(), cancellationToken);

        switch (currentSession)
        {
            case null:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            case Session { IsActive: false }:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            default:
                {
                    if (!await _userManager.IsInRoleAsync(currentSession.User!, role))
                    {
                        return Result.Failure<Session>(Errors.Auth.AccessDenied);
                    }

                    return Result.Success(currentSession);
                }
        }
    }

    protected async Task<Result<Session>> IsAbleToPerformAction(CancellationToken cancellationToken = default, params string[] enabledRoles)
    {
        var currentSession = await _dbContext.Sessions.LoadByIdAsync(_currentSessionProvider.GetCurrentSessionId(), cancellationToken);

        switch (currentSession)
        {
            case null:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            case Session { IsActive: false }:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            default:
                {
                    if (currentSession.User!.Roles.Select(e => e.Role!.Name).Any(i => enabledRoles.Contains(i)))
                    {
                        return Result.Success(currentSession);
                    }

                    return Result.Failure<Session>(Errors.Auth.AccessDenied);
                }
        }
    }

    protected async Task<Result<Session>> IsAuthenticated(CancellationToken cancellationToken = default)
    {
        var currentSession = await _dbContext.Sessions.LoadByIdAsync(_currentSessionProvider.GetCurrentSessionId(), cancellationToken);

        switch (currentSession)
        {
            case null:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            case Session { IsActive: false }:
                return Result.Failure<Session>(Errors.Auth.Unauthorized);
            default:
                {
                    return currentSession;
                }
        }
    }
}