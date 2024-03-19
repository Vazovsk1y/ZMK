using FluentValidation;
using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Validators;

public class MarkUpdateDTOValidator : AbstractValidator<MarkUpdateDTO>
{
    public MarkUpdateDTOValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.Title).NotEmpty();
        RuleFor(e => e.Weight).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(e => e.Code).NotEmpty();
        RuleFor(e => e.Count)
            .Must(e => Mark.IsValidCount(e))
            .WithMessage($"Количество должно быть больше нуля или кратное '{Mark.CountMultiplicityNumber}'.");
        RuleFor(e => e.Order).NotEmpty().GreaterThanOrEqualTo(1);
    }
}
