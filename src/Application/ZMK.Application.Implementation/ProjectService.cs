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

public class ProjectService : BaseService, IProjectService
{
    public ProjectService(
        IClock clock, 
        ILogger<BaseService> logger, 
        IServiceScopeFactory serviceScopeFactory, 
        ZMKDbContext dbContext, 
        ICurrentSessionProvider currentSessionProvider, 
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result<Guid>> AddAsync(ProjectAddDTO dTO, CancellationToken cancellationToken = default)
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

        _logger.LogInformation("Попытка добавления нового проэкта.");
        var project = dTO.ToEntity(_clock, isAbleResult.Value.UserId);
        if (await _dbContext.Projects.AnyAsync(e => e.FactoryNumber == project.FactoryNumber, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Проэкт с таким заводским номером уже существует."));
        }

        var projectSettings = dTO.ToSettingsEntity(project.Id);
        var projectAreas = dTO.Areas.Select(e => new ProjectArea { AreaId = e, ProjectId = project.Id }).ToArray();

        _dbContext.Projects.Add(project);
        _dbContext.ProjectsSettings.Add(projectSettings);
        _dbContext.ProjectsAreas.AddRange(projectAreas);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Проэкт был успешно добавлен.");
        return project.Id;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure<Guid>(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка удаления проэкта.");
        var project = await _dbContext
            .Projects
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning("Проэкта с указанным айди нет в базе данных.");
            return Result.Success();
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Проэкт был успешно удален.");
        return Result.Success();
    }

    public async Task<Result> UpdateAsync(ProjectUpdateDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Errors);
        }

        var isAbleResult = await IsAbleToPerformAction(DefaultRoles.Admin, cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return Result.Failure(isAbleResult.Errors);
        }

        _logger.LogInformation("Попытка обновления проэкта.");
        var project = await _dbContext
            .Projects
            .Include(e => e.Settings)
            .SingleOrDefaultAsync(e => e.Id == dTO.Id, cancellationToken);

        switch(project)
        {
            case null:
                return Result.Failure(Errors.NotFound("Проэкт"));
            case Project { Settings.IsEditable: false }:
                return Result.Failure(new Error(nameof(Error), "Функция редактирования не доступна для этого проэкта."));
            default:
                {
                    project.Update(dTO, _clock);
                    var previousProjectAreas = await _dbContext.ProjectsAreas.Where(e => e.ProjectId == project.Id).ToListAsync(cancellationToken);
                    var newProjectAreas = dTO.Areas.Select(e => new ProjectArea { AreaId = e, ProjectId = project.Id }).ToList();

                    _dbContext.ProjectsAreas.RemoveRange(previousProjectAreas);
                    _dbContext.ProjectsAreas.AddRange(newProjectAreas);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Проэкт был успешно обновлен.");
                    return Result.Success();
                }
        }
    }
}