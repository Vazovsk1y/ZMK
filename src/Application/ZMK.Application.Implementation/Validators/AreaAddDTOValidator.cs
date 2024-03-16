using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class AreaAddDTOValidator : AbstractValidator<AreaAddDTO>
{
    public AreaAddDTOValidator()
    {
        RuleFor(e => e.Order).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(e => e.Title).NotEmpty();
    }
}
