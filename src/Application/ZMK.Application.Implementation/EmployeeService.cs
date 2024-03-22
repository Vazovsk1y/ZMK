using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Services;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class EmployeeService : BaseService, IEmployeeService
{
    public EmployeeService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        ZMKDbContext dbContext, 
        ICurrentSessionProvider currentSessionProvider, 
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> AddAsync(EmployeeAddDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка добавления нового сотрудника.");
        var employee = new Employee
        {
            FullName = dTO.FullName.Trim(),
            Post = dTO.Post?.Trim(),
            Remark = dTO.Remark?.Trim(),
        };

        if (await _dbContext.Employees.AnyAsync(e => e.FullName == employee.FullName, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Сотрудник с таким ФИО уже существует."));
        }

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Сотрудник был успешно добавлен.");
        return employee.Id;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка удаления сотрудника.");
        var employee = await _dbContext
            .Employees
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
        {
            _logger.LogWarning("Сотрудника с указанным айди нет в базе данных.");
            return Result.Success();
        }

        if (employee.Id == isAbleResult.Value.User!.Employee!.Id)
        {
            return Result.Failure(new Error(nameof(Error), "Зайдите с другого аккаунта чтобы иметь возможность удалить данного сотрудника."));
        }

        if (await _dbContext.Users.AnyAsync(e => e.EmployeeId == employee.Id, cancellationToken) || await _dbContext.MarkCompleteEventsEmployees.AnyAsync(e => e.EmployeeId == employee.Id, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Удаление невозможно, присутствуют связанные данные."));
        }

        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Сотрудник был успешно удален.");
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(EmployeeUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return isAbleResult;
        }

        _logger.LogInformation("Попытка обновления данных сотрудника.");
        var employee = await _dbContext
            .Employees
            .SingleOrDefaultAsync(e => e.Id == dTO.Id, cancellationToken);


        if (employee is null)
        {
            return Result.Failure(Errors.NotFound("Сотрудник"));
        }

        employee.FullName = dTO.FullName.Trim();
        employee.Remark = dTO.Remark?.Trim();
        employee.Post = dTO.Post?.Trim();

        if (await _dbContext.Employees.AnyAsync(e => e.Id != employee.Id && e.FullName == employee.FullName, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error),  $"Сотрудник с таким ФИО '{employee.FullName}' уже существует."));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Данные сотрудника были успешно обновлены.");
        return Result.Success();
    }
}
