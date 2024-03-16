namespace ZMK.Domain.Entities;

public class CompleteEventEmployee
{
    public required Guid EmployeeId { get; init; }

    public required Guid EventId { get; init; }

    public Employee Employee { get; set; } = null!;

    public CompleteEvent CompleteEvent { get; set; } = null!;
}