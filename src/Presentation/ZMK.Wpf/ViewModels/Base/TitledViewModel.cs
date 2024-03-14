using CommunityToolkit.Mvvm.ComponentModel;

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
}