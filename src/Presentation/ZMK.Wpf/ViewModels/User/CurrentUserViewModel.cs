namespace ZMK.Wpf.ViewModels.User;

public class CurrentUserViewModel
{
    public required Guid Id { get; init; }

    public required string UserName { get; init; }

    public required UserViewModel.RoleInfo Role { get; init; }

    public required UserViewModel.EmployeeInfo Employee { get; init; }
}