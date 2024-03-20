namespace ZMK.Wpf.Messages;

public record MarkExecutionFilledMessage(Guid MarkId, Dictionary<Guid, double> AreasCounts);
