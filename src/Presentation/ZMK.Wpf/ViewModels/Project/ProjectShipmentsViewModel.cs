using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels.Base;
using ZMK.Wpf.ViewModels.Shipment;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels.Project;

public partial class ProjectShipmentsViewModel : 
    TitledViewModel,
    IRecipient<ShipmentAddedMessage>
{
    public ProjectViewModel SelectedProject { get; }

    [ObservableProperty]
    private ShipmentViewModel? _selectedShipment;

    [ObservableProperty]
    private ObservableCollection<ShipmentViewModel> _shipments = [];

    public ProjectShipmentsViewModel(ProjectsPanelViewModel projectsPanelViewModel)
    {
        ControlTitle = "Отгрузки";
        ArgumentNullException.ThrowIfNull(projectsPanelViewModel.SelectedProject);
        SelectedProject = projectsPanelViewModel.SelectedProject;
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<ShipmentAddWindow>();
    }

    public void Receive(ShipmentAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Shipments.Add(message.Shipment);
        });
    }

    protected override async void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var shipments = await dbContext
            .Shipments
            .Include(e => e.Creator)
            .ThenInclude(e => e.Employee)
            .AsNoTracking()
            .Where(e => e.ProjectId == SelectedProject.Id)
            .OrderBy(e => e.CreatedDate)
            .Select(e => e.ToViewModel())
            .ToListAsync();

        App.Current.Dispatcher.Invoke(() =>
        {
            SelectedShipment = null;
            Shipments = new(shipments);
        });
    }
}