using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class MarkCompleteEvent : MarkEvent
{
    public required Guid AreaId { get; set; }

    /// <summary>
    /// Кол-во, шт выполненных марок на момент создания или обновления события.
    /// </summary>
    public required double CompleteCount { get; set; }

    /// <summary>
    /// Дата выполнения марки.
    /// </summary>
    public required DateTimeOffset CompleteDate { get; set; }

    public Area Area { get; set; } = null!;

    public IEnumerable<MarkCompleteEventEmployee> Executors { get; set; } = new List<MarkCompleteEventEmployee>();
}
