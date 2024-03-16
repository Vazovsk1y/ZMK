namespace ZMK.Application.Contracts;

public record MarkExecutionDTO(Guid Id, IEnumerable<AreaExecutionDTO> AreasExecutions);
public record AreaExecutionDTO(Guid Id, IEnumerable<Guid> Executors, double Count, DateTimeOffset ExecutionDate);
