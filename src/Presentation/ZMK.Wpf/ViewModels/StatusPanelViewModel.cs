namespace ZMK.Wpf.ViewModels;

public class StatusPanelViewModel : TitledViewModel
{
    public string CurrentUserInfoRaw => $"Пользователь - {CurrentSession?.User.UserName} ({CurrentSession?.User.Employee.FullName}). Роль: {CurrentSession?.User.Role.Name}";

    public string? LoginDateRaw => $"Дата входа: {CurrentSession?.LoginDate}";

    public StatusPanelViewModel()
    {

    }
}