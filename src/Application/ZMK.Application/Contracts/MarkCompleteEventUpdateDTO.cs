namespace ZMK.Application.Contracts;

public record MarkCompleteEventUpdateDTO(Guid EventId, Guid AreaId, DateTimeOffset Date, double Count, IEnumerable<Guid> Executors, string? Remark);