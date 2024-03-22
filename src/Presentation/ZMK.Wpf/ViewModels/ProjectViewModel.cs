using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectViewModel : ModifiableViewModel<ProjectViewModel>
{
    public required Guid Id { get; init; }

    public required CreatorViewModel? Creator { get; init; }

    public required DateTimeOffset? ClosingDate { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required DateTimeOffset? ModifiedDate { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _factoryNumber = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _contractNumber;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _customer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _vendor;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _remark;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private ProjectSettingsViewModel _settings = null!;

    public override bool IsModified()
    {
        return FactoryNumber != PreviousState.FactoryNumber
            || ContractNumber != PreviousState.ContractNumber
            || Customer != PreviousState.Customer
            || Vendor != PreviousState.Vendor
            || Remark != PreviousState.Remark
            || Settings.IsModified();
    }

    public override void RollBackChanges()
    {
        FactoryNumber = PreviousState.FactoryNumber;
        ContractNumber = PreviousState.ContractNumber;
        Customer = PreviousState.Customer;
        Vendor = PreviousState.Vendor;
        Remark = PreviousState.Remark;
        Settings.RollBackChanges();
    }

    public record CreatorViewModel(Guid Id, string UserName);
}

public partial class ProjectSettingsViewModel : ModifiableViewModel<ProjectSettingsViewModel>
{
    public required Guid ProjectId { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private ObservableCollection<AreaViewModel> _areas = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _isEditable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksDeleting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksModifying;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksAdding;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _areExecutorsRequired;

    public override bool IsModified()
    {
        return IsEditable != PreviousState.IsEditable
            || AllowMarksDeleting != PreviousState.AllowMarksDeleting
            || AllowMarksModifying != PreviousState.AllowMarksModifying
            || AllowMarksAdding != PreviousState.AllowMarksAdding
            || AreExecutorsRequired != PreviousState.AreExecutorsRequired
            || !Areas.OrderBy(e => e.Title).Select(e => e.Id).SequenceEqual(PreviousState.Areas.OrderBy(e => e.Title).Select(e => e.Id));
    }

    public override void RollBackChanges()
    {
        Areas = PreviousState.Areas;
        IsEditable = PreviousState.IsEditable;
        AllowMarksDeleting = PreviousState.AllowMarksDeleting;
        AllowMarksModifying = PreviousState.AllowMarksModifying;
        AllowMarksAdding = PreviousState.AllowMarksAdding;
        AreExecutorsRequired = PreviousState.AreExecutorsRequired;
    }

    public override void SaveState()
    {
        base.SaveState();
        PreviousState.Areas = new(Areas);
    }

    public record AreaViewModel(Guid Id, string Title, int Order);
}