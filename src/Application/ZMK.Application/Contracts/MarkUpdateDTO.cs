namespace ZMK.Application.Contracts;

public record MarkUpdateDTO(Guid MarkId, string Code, string Title, int Order, double Weight, double Count);

