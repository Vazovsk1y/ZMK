namespace ZMK.Application.Contracts;

public record MarkAddDTO(Guid ProjectId, string Code, string Title, int Order, double Weight, int Count, string? Remark);