namespace ZMK.Wpf.ViewModels.Project;

public class ProjectProcessingWindowViewModel : TitledViewModel
{
    public MarksPanelViewModel MarksPanelViewModel { get; }

    public ProjectProcessingWindowViewModel(MarksPanelViewModel marksPanelViewModel)
    {
        MarksPanelViewModel = marksPanelViewModel;
        ControlTitle = "Процесс выполнения проекта";

        ActivateAllRecipients();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        DeactivateAllRecipients();
    }
}