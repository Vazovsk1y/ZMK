﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Constants;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels.Project;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectsPanelViewModel : TitledViewModel,
    IRecipient<ProjectAddedMessage>,
    IRefrashable
{
    private readonly IMemoryCache _cache;

    public ProjectsPanelViewModel(IMemoryCache cache)
    {
        _cache = cache;
    }

    public ObservableCollection<ProjectViewModel> Projects { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateProjectSettingsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProcessingCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReportsCommand))]
    private ProjectViewModel? _selectedProject;

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ProjectAddWindow>();
    }

    [RelayCommand]
    public void RollbackChanges()
    {
        var modifiedProjects = Projects.Where(e => e.IsModified()).ToList();
        if (modifiedProjects.Count == 0)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            modifiedProjects.ForEach(e => e.RollBackChanges());
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете удалить данный проект '{SelectedProject!.FactoryNumber}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await projectService.DeleteAsync(SelectedProject.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Projects.Remove(SelectedProject));
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() => SelectedProject is not null;

    [RelayCommand]
    public async Task SaveChanges()
    {
        var modifiedProjects = Projects.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedProjects.Count == 0)
        {
            return;
        }

        IsEnabled = false;
        using var scope = App.Services.CreateScope();
        var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();

        var results = new List<Result>();
        foreach (var project in modifiedProjects)
        {
            var dto = project.ToDTO();
            var updateResult = await projectService.UpdateAsync(dto);

            if (updateResult.IsFailure)
            {
                project.RollBackChanges();
            }
            else
            {
                project.SaveState();
                project.Settings.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Any(e => e.IsSuccess))
        {
            _cache.Remove(Cache.ProjectsPanelCacheKey);
            await RefreshAsync();
        }

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;
    }

    [RelayCommand(CanExecute = nameof(CanUpdateProjectSettings))]
    public async Task UpdateProjectSettings()
    {
        if (UpdateProjectSettingsCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ProjectSettingsUpdateWindow>();

        if (SelectedProject!.Settings.IsModified())
        {
            var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
            var result = await projectService.UpdateSettingsAsync(SelectedProject.Settings.ToUpdateDTO());

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                if (result.IsFailure)
                {
                    MessageBoxHelper.ShowErrorBox(result.Errors.Display());
                    SelectedProject.Settings.RollBackChanges();
                }
                else
                {
                    SelectedProject.Settings.SaveState();
                }
            });
        }
    }

    public bool CanUpdateProjectSettings() => SelectedProject is not null;

    [RelayCommand(CanExecute = nameof(CanReports))]
    public void Reports()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ProjectReportsWindow>();
    }

    public bool CanReports() => SelectedProject is not null;

    public void Receive(ProjectAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Projects.Add(message.Project);
        });
    }

    [RelayCommand(CanExecute = nameof(CanProcessing))]
    public void Processing(object selectedItem)
    {
        using var scope = App.Services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<ProjectProcessingWindow>();
        var viewModel = scope.ServiceProvider.GetRequiredService<ProjectProcessingWindowViewModel>();
        viewModel.IsActive = true;
        window.DataContext = viewModel;
        window.ShowDialog();
        viewModel.IsActive = false;
    }

    public bool CanProcessing() => SelectedProject is not null;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(Cache.ProjectsPanelCacheKey, out _))
        {
            return;
        }

        IsEnabled = false;

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var projects = await dbContext
            .Projects
            .Include(e => e.Areas)
            .ThenInclude(e => e.Area)
            .Include(e => e.Creator)
            .Include(e => e.Settings)
            .AsNoTracking()
            .Select(e => e.ToViewModel())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            object cacheStub = new();
            Projects.Clear();
            SelectedProject = null;
            Projects.AddRange(projects);
            _cache.Set(Cache.ProjectsPanelCacheKey, cacheStub, Cache.ProjectsPanelCacheExpiration);
            IsEnabled = true;
        });
    }
}