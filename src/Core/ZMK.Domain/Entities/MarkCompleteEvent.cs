using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class MarkCompleteEvent : MarkEvent
{
    public required Guid AreaId { get; init; }

    public required double CompleteCount { get; set; }

    public Area Area { get; set; } = null!;

    public IEnumerable<MarkCompleteEventEmployee> Executors { get; set; } = new List<MarkCompleteEventEmployee>();
}
