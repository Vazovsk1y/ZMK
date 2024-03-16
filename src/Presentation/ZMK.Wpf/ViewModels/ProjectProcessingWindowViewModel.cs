namespace ZMK.Wpf.ViewModels;

public class ProjectProcessingWindowViewModel : TitledViewModel
{
    public MarksPanelViewModel MarksPanelViewModel { get; }

    public ProjectProcessingWindowViewModel(MarksPanelViewModel marksPanelViewModel)
    {
        MarksPanelViewModel = marksPanelViewModel;
        ControlTitle = "Процесс выполнения проэкта";

        ActivateAllRecipients();
    }
}