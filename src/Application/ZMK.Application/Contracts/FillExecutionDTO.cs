namespace ZMK.Application.Contracts;

public record FillExecutionDTO(Guid MarkId, IEnumerable<AreaExecutionDTO> AreasExecutions);
public record AreaExecutionDTO(Guid AreaId, IEnumerable<Guid> Executors, double Count, DateTimeOffset Date, string? Remark);
