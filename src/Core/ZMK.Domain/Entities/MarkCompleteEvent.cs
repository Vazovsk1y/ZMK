using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class MarkCompleteEvent : MarkEvent
{
    public required Guid AreaId { get; set; }

    public required double CompleteCount { get; set; }

    public required DateTimeOffset CompleteDate { get; set; }

    public Area Area { get; set; } = null!;

    public IEnumerable<MarkCompleteEventEmployee> Executors { get; set; } = new List<MarkCompleteEventEmployee>();
}
