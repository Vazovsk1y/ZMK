using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public partial class MarkViewModel : ModifiableViewModel<MarkViewModel>
{
    public required Guid Id { get; init; }

    public required Guid ProjectId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required DateTimeOffset? ModifiedDate { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _code = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _title = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private int _order;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private double _weight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _remark;

    public override bool IsModified()
    {
        return Code != PreviousState.Code
            || Title != PreviousState.Title
            || Order != PreviousState.Order
            || Weight != PreviousState.Weight
            || Count != PreviousState.Count
            || Remark != PreviousState.Remark;
    }

    public override void RollBackChanges()
    {
        Code = PreviousState.Code;
        Title = PreviousState.Title;
        Order = PreviousState.Order;
        Weight = PreviousState.Weight;
        Count = PreviousState.Count;
        Remark = PreviousState.Remark;
    }
}