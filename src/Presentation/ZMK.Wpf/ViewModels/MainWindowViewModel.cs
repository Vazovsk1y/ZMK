namespace ZMK.Wpf.ViewModels;

internal class MainWindowViewModel : TitledViewModel
{
    public StatusPanelViewModel StatusPanelViewModel { get; }

    public MainWindowViewModel(StatusPanelViewModel statusPanelViewModel)
    {
        ControlTitle = App.Title;
        StatusPanelViewModel = statusPanelViewModel;
    }

    public MainWindowViewModel() 
    {
        ControlTitle = App.Title;
    }
}