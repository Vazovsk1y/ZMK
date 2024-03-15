using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class UserUpdateDTOValidator : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateDTOValidator()
    {
        RuleFor(e => e.UserName).NotEmpty();
        RuleFor(e => e.EmployeeId).NotEmpty();
        RuleFor(e => e.RoleId).NotEmpty();
        RuleFor(e => e.Id).NotEmpty();
    }
}
