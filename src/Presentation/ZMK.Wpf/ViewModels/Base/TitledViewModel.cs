using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

internal class TitledViewModel : ObservableRecipient
{
    private string? _controlTitle = "Undefined";

    public string? ControlTitle
    {
        get => _controlTitle;
        set => SetProperty(ref _controlTitle, value);
    }
}