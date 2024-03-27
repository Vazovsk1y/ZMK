using CommunityToolkit.Mvvm.ComponentModel;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Shipment;

public partial class ShipmentViewModel : ModifiableViewModel<ShipmentViewModel>
{
    public required Guid Id { get; init; }

    public required int MarksCount { get; init; }

    public required double MarksWeight { get; init; }

    public required string Creator { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private DateTime _shipmentDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _number = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _remark;

    public override bool IsModified()
    {
        return ShipmentDate != PreviousState.ShipmentDate
            || Number != PreviousState.Number
            || Remark != PreviousState.Remark;
    }

    public override void RollBackChanges()
    {
        ShipmentDate = PreviousState.ShipmentDate;
        Number = PreviousState.Number;
        Remark = PreviousState.Remark;
    }
}