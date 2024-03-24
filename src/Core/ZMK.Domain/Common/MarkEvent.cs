using ZMK.Domain.Entities;

namespace ZMK.Domain.Common;

public class MarkEvent : Entity, IMarkAuditable
{
    public required Guid CreatorId { get; init; }

    public required Guid MarkId { get; init; }

    /// <summary>
    /// Дата создания события.
    /// </summary>
    public required DateTimeOffset CreatedDate { get; init; }

    public required EventType EventType { get; init; }

    /// <summary>
    /// Количество марок, на момент создания или обновления события.
    /// </summary>
    public required double MarkCount { get; set; }

    /// <summary>
    /// Название марки, на момент создания или обновления события.
    /// </summary>
    public required string MarkTitle { get; set; }

    /// <summary>
    /// Код марки, на момент создания или обновления события.
    /// </summary>
    public required string MarkCode { get; set; }

    /// <summary>
    /// Очередность марки, на момент создания или обновления события.
    /// </summary>
    public required int MarkOrder { get; set; }

    /// <summary>
    /// Вес марки в килограммах, на момент создания или обновления события.
    /// </summary>
    public required double MarkWeight { get; set; }

    public string? Remark { get; set; }

    #region --Navigation--

    public User Creator { get; set; } = null!;

    public Mark Mark { get; set; } = null!;

    #endregion
}

public enum EventType
{
    Create,
    Modify,
    Complete,
}