using CommunityToolkit.Mvvm.ComponentModel;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Employee;

public partial class EmployeeViewModel : ModifiableViewModel<EmployeeViewModel>
{
    public required Guid Id { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _fullName = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _post = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string? _remark = null!;

    public override bool IsModified()
    {
        return FullName != PreviousState.FullName
            || Post != PreviousState.Post
            || Remark != PreviousState.Remark;
    }

    public override void RollBackChanges()
    {
        FullName = PreviousState.FullName;
        Post = PreviousState.Post;
        Remark = PreviousState.Remark;
    }
}