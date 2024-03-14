namespace ZMK.Application.Contracts;

public record UserAddDTO(string UserName, string Password, Guid RoleId, Guid EmployeeId);

