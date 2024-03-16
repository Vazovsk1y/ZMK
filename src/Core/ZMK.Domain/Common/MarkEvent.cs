using ZMK.Domain.Entities;

namespace ZMK.Domain.Common;

public class MarkEvent : Entity, IAuditable
{
    public required Guid CreatorId { get; init; }

    public required Guid MarkId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required EventType EventType { get; init; }

    public DateTimeOffset? ModifiedDate { get; set; }

    public required double Count { get; set; }

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