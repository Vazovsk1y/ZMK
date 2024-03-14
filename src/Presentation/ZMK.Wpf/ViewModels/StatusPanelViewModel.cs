namespace ZMK.Wpf.ViewModels;

internal class StatusPanelViewModel : TitledViewModel
{
    public string CurrentUserInfoRaw => $"Пользователь - {CurrentSession?.User.UserName} ({CurrentSession?.User.Employee.FullName}). Роль: {CurrentSession?.User.Roles.FirstOrDefault()}";

    public string? LoginDateRaw => $"Дата входа: {CurrentSession?.LoginDate}";

    public StatusPanelViewModel()
    {

    }
}