using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;
using ZMK.Wpf.ViewModels.User;

namespace ZMK.Wpf.ViewModels.Base;

public partial class TitledViewModel : ObservableRecipient
{
    public CurrentSessionViewModel? CurrentSession { get; } = App.CurrentSession;

    [ObservableProperty]
    private string? _controlTitle = "Undefined";

    [ObservableProperty]
    private bool _isEnabled = true;

    protected void ActivateAllRecipients()
    {
        var type = GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.GetValue(this) is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = true;
            }
        }
    }

    protected void DeactivateAllRecipients()
    {
        var type = GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.GetValue(this) is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = false;
            }
        }
    }
}