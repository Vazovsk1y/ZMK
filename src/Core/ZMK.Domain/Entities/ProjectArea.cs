namespace ZMK.Domain.Entities;

public class ProjectArea
{
    public required Guid ProjectId { get; init; }

    public required Guid AreaId { get; init; }

    public Project Project { get; set; } = null!;

    public Area Area { get; set; } = null!;
}