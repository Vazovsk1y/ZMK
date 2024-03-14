﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.Domain.Constants;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class UserService : BaseService, IUserService
{
    public UserService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        UserManager<User> userManager,
        ICurrentSessionProvider currentSessionProvider) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> AddAsync(UserAddDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Guid>(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(cancellationToken, DefaultRoles.Admin).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        using var transaction = _dbContext.Database.BeginTransaction();
        _logger.LogInformation("Попытка добавления нового пользователя. Начата транзакция.");

        var user = new User
        {
            EmployeeId = dTO.EmployeeId,
            UserName = dTO.UserName.Trim(),
        };
        try
        {
            var result = string.IsNullOrWhiteSpace(dTO.Password) ?
            await _userManager.CreateAsync(user)
            :
            await _userManager.CreateAsync(user, dTO.Password);

            if (!result.Succeeded)
            {
                return Result.Failure<Guid>(result.Errors.ToErrors());
            }

            var role = await _dbContext.Roles.SingleAsync(e => e.Id == dTO.RoleId, cancellationToken);
            _dbContext.UserRoles.Add(new UserRole { RoleId = role.Id, UserId = user.Id });

            await _dbContext.SaveChangesAsync(cancellationToken);
            transaction.Commit();
        }
        catch (Exception ex) 
        {
            transaction.Rollback();
            _logger.LogError(ex, "Что-то пошло не так во время добавления нового пользователя. Транзакция не была зафиксирована.");
            return Result.Failure<Guid>(new Error(nameof(Exception), "Что-то пошло не так во время добавления нового пользователя. Транзакция не была зафиксирована."));
        }

        _logger.LogInformation("Новый пользователь был успешно добавлен.");
        return Result.Success(user.Id);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAbleResult = await IsAbleToPerformAction(cancellationToken, DefaultRoles.Admin).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return isAbleResult;
        }

        _logger.LogInformation("Попытка удаления пользователя.");
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            _logger.LogWarning("Пользователя с указанным айди нет в базе данных.");
            return Result.Success();
        }

        if (user.Id == isAbleResult.Value.UserId)
        {
            return Result.Failure(new Error(nameof(Error),"Зайдите с другого аккаунта чтобы иметь возможность удалить данного пользователя."));
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Пользователь был успешно удален.");
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(UserUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var isAbleResult = await IsAbleToPerformAction(cancellationToken, DefaultRoles.Admin);
        if (isAbleResult.IsFailure)
        {
            return isAbleResult;
        }

        var user = await _userManager.FindByIdAsync(dTO.Id.ToString());
        if (user is null)
        {
            return Result.Failure(Errors.NotFound("Пользователь"));
        }

        using var transaction = _dbContext.Database.BeginTransaction();
        _logger.LogInformation("Попытка обновления пользователя. Начата транзакция.");
        try
        {
            var newRole = await _dbContext.Roles.SingleAsync(e => e.Id == dTO.RoleId, cancellationToken);
            var userRole = await _dbContext.UserRoles.SingleAsync(e => e.UserId == user.Id && e.RoleId == e.RoleId, cancellationToken);
            _dbContext.UserRoles.Remove(userRole);
            await _dbContext.SaveChangesAsync(cancellationToken);

            user.UserName = dTO.UserName.Trim();
            user.EmployeeId = dTO.EmployeeId;

            IdentityResult changePasswordResult;
            if (!string.IsNullOrWhiteSpace(dTO.Password))
            {
                if (await _userManager.HasPasswordAsync(user))
                {
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dTO.Password);
                    changePasswordResult = IdentityResult.Success;
                }
                else
                {
                    changePasswordResult = await _userManager.AddPasswordAsync(user, dTO.Password);
                }
            }
            else
            {
                user.PasswordHash = null;
                changePasswordResult = IdentityResult.Success;
            }
            
            if (!changePasswordResult.Succeeded)
            {
                return Result.Failure(changePasswordResult.Errors.ToErrors());
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Result.Failure(updateResult.Errors.ToErrors());
            }

            _dbContext.UserRoles.Add(new UserRole { RoleId = newRole.Id, UserId = user.Id });
            await _dbContext.SaveChangesAsync(cancellationToken);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Что-то пошло не так во время обновления пользователя. Транзакция не была зафиксирована.");
            return Result.Failure(new Error(nameof(Exception), "Что-то пошло не так во время обновления пользователя. Транзакция не была зафиксирована."));
        }

        _logger.LogInformation("Пользователь был успешно обновлен.");
        return Result.Success();
    }
}