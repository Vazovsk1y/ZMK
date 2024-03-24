using CommunityToolkit.Mvvm.ComponentModel;
using ZMK.Application.Implementation.Extensions;
using ZMK.Wpf.Extensions;

namespace ZMK.Wpf.ViewModels;

public partial class MarkViewModel : ModifiableViewModel<MarkViewModel>
{
    public required Guid Id { get; init; }

    public required Guid ProjectId { get; init; }

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
    [NotifyPropertyChangedFor(nameof(TotalWeight))]
    private double _weight;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    [NotifyPropertyChangedFor(nameof(TotalWeight))]
    private double _count;

    [ObservableProperty]
    private double _left;

    [ObservableProperty]
    private double _complete;

    public double TotalWeight => (Weight * Count).RoundForReport();

    public override bool IsModified()
    {
        return Code != PreviousState.Code
            || Title != PreviousState.Title
            || Order != PreviousState.Order
            || Weight != PreviousState.Weight
            || Count != PreviousState.Count;
    }

    public override void RollBackChanges()
    {
        Code = PreviousState.Code;
        Title = PreviousState.Title;
        Order = PreviousState.Order;
        Weight = PreviousState.Weight;
        Count = PreviousState.Count;
        OnPropertyChanged(nameof(TotalWeight));
    }
}