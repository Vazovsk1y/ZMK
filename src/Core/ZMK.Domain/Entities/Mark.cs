using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Mark : Entity, IAuditable
{
    public required string Code { get; set; }

    public required string Title { get; set; }

    public required int Order { get; set; }

    public required double Weight { get; set; }

    public required int Count { get; set; }

    public required Guid ProjectId { get; init; }

    public Project Project { get; set; } = null!;

    public string? Remark { get; set; }

    public required DateTimeOffset CreatedDate { get; init; }

    public DateTimeOffset? ModifiedDate { get; set; }
}
