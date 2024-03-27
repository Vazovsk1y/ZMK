namespace ZMK.Wpf.ViewModels.Mark;

public class MarkCreateOrModifyEventViewModel : MarkEventViewModel
{
    public required string MarkCode { get; init; }

    public required string MarkTitle { get; init; }

    public required double MarkWeight { get; init; }

    public required int MarkOrder { get; init; }
}
