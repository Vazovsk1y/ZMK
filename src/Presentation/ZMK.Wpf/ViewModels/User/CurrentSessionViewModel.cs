namespace ZMK.Wpf.ViewModels.User;


public class CurrentSessionViewModel
{
    public required Guid Id { get; init; }

    public required CurrentUserViewModel User { get; init; }

    public DateTime LoginDate { get; } = DateTime.Now;
}