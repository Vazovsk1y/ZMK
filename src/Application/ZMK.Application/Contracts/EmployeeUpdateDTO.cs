namespace ZMK.Application.Contracts;

public record EmployeeUpdateDTO(Guid Id, string FullName,  string? Post, string? Remark);

