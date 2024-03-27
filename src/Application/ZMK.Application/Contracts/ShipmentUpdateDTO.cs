namespace ZMK.Application.Contracts;

public record ShipmentUpdateDTO(Guid ShipmentId, DateOnly ShipmentDate, string Number, string? Remark);