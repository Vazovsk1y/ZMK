using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class UserAddDTOValidator : AbstractValidator<UserAddDTO>
{
    public UserAddDTOValidator()
    {
        RuleFor(e => e.UserName).NotEmpty();
        RuleFor(e => e.EmployeeId).NotEmpty();
        RuleFor(e => e.RoleId).NotEmpty();
    }
}