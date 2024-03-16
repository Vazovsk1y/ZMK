using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace ZMK.Wpf.ViewModels;

public class TitledViewModel : ObservableRecipient
{
    public CurrentSessionViewModel? CurrentSession { get; } = App.CurrentSession;

    private string? _controlTitle = "Undefined";

    public string? ControlTitle
    {
        get => _controlTitle;
        set => SetProperty(ref _controlTitle, value);
    }

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
}