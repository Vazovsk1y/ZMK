using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class EmployeeUpdateDTOValidator : AbstractValidator<EmployeeUpdateDTO>
{
    public EmployeeUpdateDTOValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.FullName).NotEmpty();
    }
}