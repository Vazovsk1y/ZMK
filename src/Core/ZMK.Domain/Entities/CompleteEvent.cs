using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class CompleteEvent : MarkEvent
{
    public required Guid AreaId { get; init; }

    public Area Area { get; set; } = null!;

    public IEnumerable<CompleteEventEmployee> Executors { get; set; } = new List<CompleteEventEmployee>();
}
