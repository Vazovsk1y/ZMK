﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public partial class UserViewModel : ModifiableViewModel<UserViewModel>
{
    public required Guid Id { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _userName = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private RoleViewModel _role = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private EmployeeViewModel _employee = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private string _password = null!;

    public override bool IsModified()
    {
        return UserName != PreviousState.UserName
            || Role != PreviousState.Role
            || Employee != PreviousState.Employee
            || !string.IsNullOrWhiteSpace(Password);
    }

    public override void RollBackChanges()
    {
        UserName = PreviousState.UserName;
        Role = PreviousState.Role;
        Employee = PreviousState.Employee;
        Password = string.Empty;
    }

    public override void SaveState()
    {
        base.SaveState();
        Password = string.Empty;
    }

    public record EmployeeViewModel(Guid Id, string FullName);

    public record RoleViewModel(Guid Id, string Name);
}

