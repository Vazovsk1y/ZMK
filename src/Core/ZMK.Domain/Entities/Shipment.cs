using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

/// <summary>
/// Отгрузка.
/// </summary>
public class Shipment : Entity
{
    public required Guid ProjectId { get; init; }

    public required Guid CreatorId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required string Number { get; set; }

    public string? Remark { get; set; }

    public Project Project { get; set; } = null!;

    public User Creator { get; set; } = null!;
}