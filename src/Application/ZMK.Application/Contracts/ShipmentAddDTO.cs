namespace ZMK.Application.Contracts;

public record ShipmentAddDTO(Guid ProjectId, string Number, string? Remark);