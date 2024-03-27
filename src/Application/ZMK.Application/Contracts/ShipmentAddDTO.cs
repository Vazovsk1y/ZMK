namespace ZMK.Application.Contracts;

public record ShipmentAddDTO(Guid ProjectId, DateOnly ShipmentDate, string Number, string? Remark);