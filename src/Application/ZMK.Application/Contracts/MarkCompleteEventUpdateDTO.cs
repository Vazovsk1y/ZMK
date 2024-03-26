namespace ZMK.Application.Contracts;

public record MarkCompleteEventUpdateDTO(Guid EventId, Guid AreaId, DateOnly CompleteDate, double Count, IEnumerable<Guid> Executors, string? Remark);