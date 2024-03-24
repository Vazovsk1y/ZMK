using Microsoft.AspNetCore.Identity;
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

        _logger.LogInformation("Попытка добавления нового проекта.");
        var project = dTO.ToEntity(_clock, isAbleResult.Value.UserId);
        if (await _dbContext.Projects.AnyAsync(e => e.FactoryNumber == project.FactoryNumber, cancellationToken))
        {
            return Result.Failure<Guid>(new Error(nameof(Error), "Проект с таким заводским номером уже существует."));
        }

        var projectSettings = dTO.ToSettingsEntity(project.Id);
        var projectAreas = dTO.Areas.Select(e => new ProjectArea { AreaId = e, ProjectId = project.Id }).ToList();

        _dbContext.Projects.Add(project);
        _dbContext.ProjectsSettings.Add(projectSettings);
        _dbContext.ProjectsAreas.AddRange(projectAreas);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Проект был успешно добавлен.");
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

        _logger.LogInformation("Попытка удаления проекта.");
        var project = await _dbContext
            .Projects
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning("Проекта с указанным айди нет в базе данных.");
            return Result.Success();
        }

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Проект был успешно удален.");
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

        _logger.LogInformation("Попытка обновления информации о проекте.");
        var project = await _dbContext
            .Projects
            .Include(e => e.Settings)
            .SingleOrDefaultAsync(e => e.Id == dTO.ProjectId, cancellationToken);

        project?.Update(dTO, _clock);
        switch (project)
        {
            case null:
                return Result.Failure(Errors.NotFound("Проект"));
            case Project { Settings.IsEditable: false }:
                return Result.Failure(new Error(nameof(Error), $"Функция редактирования отключена для '{project.FactoryNumber}'."));
            case Project when await _dbContext.Projects.AnyAsync(e => e.Id != project.Id && e.FactoryNumber == project.FactoryNumber, cancellationToken):
                return Result.Failure<Guid>(new Error(nameof(Error), $"Проект с таким заводским '{project.FactoryNumber}' номером уже существует."));
            default:
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Информация о проекте была успешно обновлена.");
                    return Result.Success();
                }
        }
    }

    public async Task<Result> UpdateSettingsAsync(ProjectSettingsUpdateDTO dTO, CancellationToken cancellationToken = default)
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

        _logger.LogInformation("Попытка обновления настроек проекта.");
        var settings = await _dbContext
            .ProjectsSettings
            .SingleOrDefaultAsync(e => e.ProjectId == dTO.ProjectId, cancellationToken);

        if (settings is null)
        {
            _logger.LogWarning("Настройки для указанного проекта не найдены в базе данных.");
            return Result.Success();
        }

        settings.IsEditable = dTO.IsEditable;
        settings.AllowMarksDeleting = dTO.AllowMarksDeleting;
        settings.AllowMarksAdding = dTO.AllowMarksAdding;
        settings.AllowMarksModifying = dTO.AllowMarksModifying;
        settings.AreExecutorsRequired = dTO.AreExecutorsRequired;

        var previousProjectAreas = await _dbContext.ProjectsAreas.Where(e => e.ProjectId == dTO.ProjectId).ToListAsync(cancellationToken);
        var newProjectAreas = dTO.Areas.Select(e => new ProjectArea { AreaId = e, ProjectId = dTO.ProjectId }).ToList();

        _dbContext.ProjectsAreas.RemoveRange(previousProjectAreas);
        _dbContext.ProjectsAreas.AddRange(newProjectAreas);

        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Настройки проекта были успешно обновлены.");
        return Result.Success();
    }
}