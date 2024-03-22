using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectViewModel : ModifiableViewModel<ProjectViewModel>
{
    public required Guid Id { get; init; }

    public required CreatorInfo? Creator { get; init; }

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

    public record CreatorInfo(Guid Id, string UserName);
}