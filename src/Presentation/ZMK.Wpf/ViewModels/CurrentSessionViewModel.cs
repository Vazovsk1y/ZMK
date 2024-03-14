using System.Collections.ObjectModel;

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

    public required ObservableCollection<string> Roles { get; init; }

    public required CurrentEmployeeViewModel Employee { get; init; }
}

public partial class CurrentEmployeeViewModel
{
    public required Guid Id { get; init; }

    public required string FullName { get; init; }
}