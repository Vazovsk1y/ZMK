namespace ZMK.Application.Contracts;

public record UserUpdateDTO(Guid Id, string UserName, string Password, Guid RoleId, Guid EmployeeId);

