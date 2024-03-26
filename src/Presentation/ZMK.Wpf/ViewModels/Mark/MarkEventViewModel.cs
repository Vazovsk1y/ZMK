using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public partial class MarkEventViewModel : ObservableObject
{
    public const string CompleteEventType = "Выполнено";

    public const string CreateEventType = "Создано";

    public const string ModifyEventType = "Изменено";

    public const string CommonEventType = "Общий";

    public required Guid Id { get; init; }

    public virtual DateTime Date { get; set; }

    public virtual string DisplayDate => Date.ToString("dd.MM.yyyy HH:mm");

    public virtual double MarkCount { get; set; }

    public required string CreatorUserNameAndEmployeeFullName { get; init; }

    [ObservableProperty]
    private string _commonTitle = null!;

    public required string EventType { get; init; }

    [ObservableProperty]
    private string _displayEventType = CommonEventType;

    public virtual string? Remark { get; set; }

    [ObservableProperty]
    private bool _isEditable;
}
