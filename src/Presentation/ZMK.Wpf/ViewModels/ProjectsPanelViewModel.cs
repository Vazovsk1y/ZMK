using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectsPanelViewModel : ObservableRecipient,
    IRecipient<ProjectAddedMessage>,
    IRefrashable
{
    public ObservableCollection<ProjectViewModel> Projects { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(ProjectSettingsCommand))]
    private ProjectViewModel? _selectedProject;

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ProjectAddWindow>();
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

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете удалить данный проэкт '{SelectedProject!.FactoryNumber}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await projectService.DeleteAsync(SelectedProject.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Projects.Remove(SelectedProject));
                MessageBoxHelper.ShowInfoBox("Проэкт успешно удален.");
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

        if (results.Where(e => e.IsSuccess).Any())
        {
            MessageBoxHelper.ShowInfoBox($"Информация об {results.Where(e => e.IsSuccess).Count()} проэктах была обновлена успешно.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(results.Where(e => e.IsFailure).SelectMany(e => e.Errors).Display());
        }
    }

    [RelayCommand(CanExecute = nameof(CanProjectSettings))]
    public void ProjectSettings()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ProjectSettingsUpdateWindow>();
        SelectedProject!.UpdatableSign = SelectedProject.Settings.IsModified() ? new UpdatableSign() : null;
    }

    public bool CanProjectSettings() => SelectedProject is not null;

    public void Receive(ProjectAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Projects.Add(message.Project);
            MessageBoxHelper.ShowInfoBox("Проэкт был успешно добавлен.");
        });
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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
            Projects.Clear();
            SelectedProject = null;
            Projects.AddRange(projects);
        });
    }
}