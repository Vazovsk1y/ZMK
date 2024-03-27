using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels.Shipment;

public partial class ShipmentViewModel : ObservableObject
{
    public required Guid Id { get; init; }

    public required int MarksCount { get; init; }

    public required double MarksWeight { get; init; }

    public required string Creator { get; init; }

    [ObservableProperty]
    private DateTime _shipmentDate;

    [ObservableProperty]
    private string _number = null!;

    [ObservableProperty]
    private string? _remark;
}