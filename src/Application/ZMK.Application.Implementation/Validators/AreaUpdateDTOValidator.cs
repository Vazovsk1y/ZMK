using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class AreaUpdateDTOValidator : AbstractValidator<AreaUpdateDTO>
{
    public AreaUpdateDTOValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.Order).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(e => e.Title).NotEmpty();
    }
}