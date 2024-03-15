using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class MarkAddDTOValidator : AbstractValidator<MarkAddDTO>
{
    public MarkAddDTOValidator()
    {
        RuleFor(e => e.ProjectId).NotEmpty();
        RuleFor(e => e.Title).NotEmpty();
        RuleFor(e => e.Weight).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(e => e.Code).NotEmpty();
        RuleFor(e => e.Count).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(e => e.Order).NotEmpty().GreaterThanOrEqualTo(1);
    }
}