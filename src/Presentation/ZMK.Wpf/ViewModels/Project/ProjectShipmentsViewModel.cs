using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
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

    [RelayCommand]
    public async Task SaveChanges()
    {
        var modifiedShipments = Shipments.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedShipments.Count == 0)
        {
            return;
        }

        IsEnabled = false;
        using var scope = App.Services.CreateScope();
        var shipmentService = scope.ServiceProvider.GetRequiredService<IShipmentService>();

        var results = new List<Result>();
        foreach (var shipment in modifiedShipments)
        {
            var dto = new ShipmentUpdateDTO(shipment.Id, DateOnly.FromDateTime(shipment.ShipmentDate), shipment.Number, shipment.Remark);
            var updateResult = await shipmentService.UpdateAsync(dto);
            if (updateResult.IsFailure)
            {
                shipment.RollBackChanges();
            }
            else
            {
                shipment.SaveState();
            }

            results.Add(updateResult);
        }

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;
    }

    [RelayCommand]
    public void RollbackChanges()
    {
        var modifiedShipments = Shipments.Where(e => e.IsModified()).ToList();
        if (modifiedShipments.Count == 0)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            modifiedShipments.ForEach(e => e.RollBackChanges());
        }
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