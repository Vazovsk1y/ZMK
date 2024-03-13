using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public abstract class BaseService
{
    protected readonly IClock _clock;
    protected readonly ILogger _logger;
    protected readonly IServiceScopeFactory _serviceScopeFactory;
    protected readonly ZMKDbContext _dbContext;

    protected BaseService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext)
    {
        _clock = clock;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _dbContext = dbContext;
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
}