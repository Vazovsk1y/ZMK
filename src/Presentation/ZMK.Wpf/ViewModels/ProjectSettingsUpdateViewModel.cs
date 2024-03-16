using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Services;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectSettingsUpdateViewModel : DialogViewModel
{
    public ProjectSettingsViewModel ProjectSettingsViewModel { get; }

    public ObservableCollection<SelectableAreaViewModel> Areas { get; } = [];

    public ProjectSettingsUpdateViewModel(
        IUserDialogService userDialogService,
        ProjectsPanelViewModel projectsPanelViewModel) : base(userDialogService)
    {
        ProjectSettingsViewModel = projectsPanelViewModel.SelectedProject!.Settings;
        ControlTitle = "Настройки проэкта";
    }

    protected override Task Accept(object action)
    {
        var selectedAreas = Areas.Where(e => e.IsSelected).ToList();
        if (AcceptCommand.IsRunning)
        {
            return Task.CompletedTask;
        }

        if (!selectedAreas.OrderBy(e => e.Title).Select(e => e.Id).SequenceEqual(ProjectSettingsViewModel.Areas.OrderBy(e => e.Title).Select(e => e.Id)))
        {
            ProjectSettingsViewModel.Areas = new(selectedAreas.Select(e => new ProjectSettingsViewModel.AreaViewModel(e.Id, e.Title, e.Order)));
        }
        
        _dialogService.CloseDialog();
        return Task.CompletedTask;
    }

    protected override bool CanAccept(object p)
    {
        return true;
    }

    protected override Task Cancel(object p)
    {
        ProjectSettingsViewModel.RollBackChanges();
        return base.Cancel(p);
    }

    protected async override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var areas = await dbContext
            .Areas
            .AsNoTracking()
            .OrderBy(e => e.Order)
            .Select(e => new SelectableAreaViewModel { Id = e.Id, Title = e.Title, Order = e.Order })
            .ToListAsync();

        var ids = ProjectSettingsViewModel.Areas.Select(e => e.Id).ToList();
        areas.ForEach(e =>
        {
            if (ids.Contains(e.Id))
            {
                e.IsSelected = true;
            }
        });

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Areas.AddRange(areas);
        });
    }

    public partial class SelectableAreaViewModel : ObservableObject
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required int Order { get; init; }

        [ObservableProperty]
        private bool _isSelected;
    }
}