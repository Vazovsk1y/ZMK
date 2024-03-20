namespace ZMK.Domain.Entities;

public class MarkCompleteEventEmployee
{
    public required Guid EmployeeId { get; init; }

    public required Guid EventId { get; init; }

    public Employee Employee { get; set; } = null!;

    public MarkCompleteEvent MarkCompleteEvent { get; set; } = null!;
}