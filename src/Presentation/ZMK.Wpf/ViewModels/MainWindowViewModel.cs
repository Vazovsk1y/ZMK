using CommunityToolkit.Mvvm.ComponentModel;
using System.Reflection;

namespace ZMK.Wpf.ViewModels;

internal class MainWindowViewModel : TitledViewModel
{
    public StatusPanelViewModel StatusPanelViewModel { get; }

    public UsersPanelViewModel UsersPanelViewModel { get; }

    public MainWindowViewModel(
        StatusPanelViewModel statusPanelViewModel, 
        UsersPanelViewModel usersPanelViewModel)
    {
        ControlTitle = App.Title;
        StatusPanelViewModel = statusPanelViewModel;
        UsersPanelViewModel = usersPanelViewModel;

        ActivateAllRecipients();
    }

    public MainWindowViewModel() 
    {
        ControlTitle = App.Title;
    }

    private void ActivateAllRecipients()
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