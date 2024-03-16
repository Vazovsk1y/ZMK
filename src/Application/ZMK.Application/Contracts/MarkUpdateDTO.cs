namespace ZMK.Application.Contracts;

public record MarkUpdateDTO(Guid Id, string Code, string Title, int Order, double Weight, int Count);

