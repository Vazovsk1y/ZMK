using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels.Base;
using ZMK.Wpf.ViewModels.Project;

namespace ZMK.Wpf.ViewModels.Shipment;

public partial class ShipmentAddViewModel : DialogViewModel
{
    public ProjectViewModel SelectedProject { get; }

    [ObservableProperty]
    private DateTime _shipmentDate = DateTime.Now;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _number = null!;

    [ObservableProperty]
    private string? _remark;

    public ShipmentAddViewModel(
        IUserDialogService userDialogService,
        ProjectsPanelViewModel projectsPanelViewModel) : base(userDialogService)
    {
        ControlTitle = "Создание отгрузки";
        ArgumentNullException.ThrowIfNull(projectsPanelViewModel.SelectedProject);
        SelectedProject = projectsPanelViewModel.SelectedProject;
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IShipmentService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();
        var dto = new ShipmentAddDTO(SelectedProject.Id, DateOnly.FromDateTime(ShipmentDate), Number!, Remark);
        var result = await service.AddAsync(dto);
        if (result.IsSuccess)
        {
            var shipment = await dbContext
                .Shipments
                .Include(e => e.Creator)
                .ThenInclude(e => e.Employee)
                .SingleAsync(e => e.Id == result.Value);

            Messenger.Send(new ShipmentAddedMessage(shipment.ToViewModel()));
            App.Current.Dispatcher.Invoke(_dialogService.CloseDialog);
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p) => !string.IsNullOrWhiteSpace(Number);

    protected override async void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        int shipmentsCount = await dbContext.Shipments.Where(e => e.ProjectId == SelectedProject.Id).CountAsync();

        App.Current.Dispatcher.Invoke(() =>
        {
            Number = shipmentsCount == 0 ? $"{SelectedProject.FactoryNumber}-1" : $"{SelectedProject.FactoryNumber}-{++shipmentsCount}";
        });
    }
}