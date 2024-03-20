using ZMK.Domain.Entities;

namespace ZMK.Domain.Common;

public class MarkEvent : Entity, IMarkAuditable
{
    public required Guid CreatorId { get; init; }

    public required Guid MarkId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required EventType EventType { get; init; }

    public required double MarkCount { get; init; }

    public required string MarkTitle { get; init; }

    public required string MarkCode { get; init; }

    public required int MarkOrder { get; init; }

    public required double MarkWeight { get; init; }

    public string? Remark { get; init; }

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