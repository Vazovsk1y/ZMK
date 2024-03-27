using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

/// <summary>
/// Отгрузка.
/// </summary>
public class Shipment : Entity
{
    public required Guid ProjectId { get; init; }

    public required Guid CreatorId { get; init; }

    /// <summary>
    /// Дата создания погрузки.
    /// </summary>
    public required DateTimeOffset CreatedDate { get; init; }

    /// <summary>
    /// Дата погрузки.
    /// </summary>
    public required DateTimeOffset ShipmentDate { get; set; }

    public required string Number { get; set; }

    public string? Remark { get; set; }

    public Project Project { get; set; } = null!;

    public User Creator { get; set; } = null!;
}