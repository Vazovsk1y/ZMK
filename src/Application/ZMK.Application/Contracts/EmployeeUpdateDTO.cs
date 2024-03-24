namespace ZMK.Application.Contracts;

public record EmployeeUpdateDTO(Guid EmployeeId, string FullName,  string? Post, string? Remark);

