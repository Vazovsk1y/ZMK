﻿namespace ZMK.Application.Contracts;

public record FillMarkExecutionDTO(Guid MarkId, IEnumerable<AreaExecutionDTO> Executions);
public record AreaExecutionDTO(Guid AreaId, IEnumerable<Guid> Executors, double Count, DateTimeOffset Date, string? Remark);