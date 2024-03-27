using CommunityToolkit.Mvvm.ComponentModel;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Area;

public partial class AreaViewModel : ModifiableViewModel<AreaViewModel>
{
    public required Guid Id { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public string _title = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public int _order;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public string? _remark;

    public override bool IsModified()
    {
        return Title != PreviousState.Title
            || Order != PreviousState.Order
            || Remark != PreviousState.Remark;
    }

    public override void RollBackChanges()
    {
        Title = PreviousState.Title;
        Order = PreviousState.Order;
        Remark = PreviousState.Remark;
    }
}
