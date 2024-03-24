using ZMK.Domain.Common;

namespace ZMK.Domain.Entities;

public class Mark : Entity
{
    public const double CountMultiplicityNumber = 0.5d;

    public required string Code { get; set; }

    public required string Title { get; set; }

    public required int Order { get; set; }

    /// <summary>
    /// Вес марки в килограммах.
    /// </summary>
    public required double Weight { get; set; }

    public required Guid ProjectId { get; init; }

    public Project Project { get; set; } = null!;

    public required double Count { get; set; }

    public static bool IsValidCount(double number)
    {
        return (number > 0 && number % 1 == 0) || (number > 0 && number % CountMultiplicityNumber == 0);
    }
}