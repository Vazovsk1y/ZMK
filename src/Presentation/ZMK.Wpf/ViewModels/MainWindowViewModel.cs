namespace ZMK.Wpf.ViewModels;

internal class MainWindowViewModel : TitledViewModel
{
    public MainWindowViewModel() 
    {
        ControlTitle = App.Title;
    }
}