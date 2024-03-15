using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class EmployeeAddDTOValidator : AbstractValidator<EmployeeAddDTO>
{
    public EmployeeAddDTOValidator()
    {
        RuleFor(e => e.FullName).NotEmpty();
    }
}
