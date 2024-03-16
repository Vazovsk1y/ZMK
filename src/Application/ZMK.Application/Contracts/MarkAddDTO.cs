namespace ZMK.Application.Contracts;

public record MarkAddDTO(Guid ProjectId, string Code, string Title, int Order, double Weight, double Count, string? Remark);