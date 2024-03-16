using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Mark : Entity
{
    public const double CountMultiplicityNumber = 0.5d;

    public required string Code { get; set; }

    public required string Title { get; set; }

    public required int Order { get; set; }

    public required double Weight { get; set; }

    public required Guid ProjectId { get; init; }

    public Project Project { get; set; } = null!;

    public required double Count { get; set; }
}