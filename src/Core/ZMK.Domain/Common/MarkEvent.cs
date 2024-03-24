using ZMK.Domain.Entities;

namespace ZMK.Domain.Common;

public class MarkEvent : Entity, IMarkAuditable
{
    public required Guid CreatorId { get; init; }

    public required Guid MarkId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required EventType EventType { get; init; }

    public required double MarkCount { get; set; }

    public required string MarkTitle { get; set; }

    public required string MarkCode { get; set; }

    public required int MarkOrder { get; set; }

    /// <summary>
    /// Вес марки в килограммах.
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