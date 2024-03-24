namespace ZMK.Application.Contracts;

public record UserUpdateDTO(Guid UserId, string UserName, string Password, Guid RoleId, Guid EmployeeId);

