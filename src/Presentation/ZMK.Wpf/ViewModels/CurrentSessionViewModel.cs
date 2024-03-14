namespace ZMK.Wpf.ViewModels;


public class CurrentSessionViewModel
{
    public required Guid Id { get; init; }

    public required CurrentUserViewModel User { get; init; }

    public DateTime LoginDate { get; } = DateTime.Now;
}
public class CurrentUserViewModel
{
    public required Guid Id { get; init; }

    public required string UserName { get; init; }

    public required UserViewModel.RoleViewModel Role { get; init; }

    public required UserViewModel.EmployeeViewModel Employee { get; init; }
}