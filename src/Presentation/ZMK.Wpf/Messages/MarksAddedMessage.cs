using ZMK.Wpf.ViewModels;

namespace ZMK.Wpf.Messages;

public record MarksAddedMessage(IEnumerable<MarkViewModel> Marks);
