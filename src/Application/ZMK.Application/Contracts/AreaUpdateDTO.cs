namespace ZMK.Application.Contracts;

public record AreaUpdateDTO(Guid AreaId, int Order, string Title, string? Remark);

