using FluentValidation;
using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Validators;

public class MarkCompleteEventUpdateDTOValidator : AbstractValidator<MarkCompleteEventUpdateDTO>
{
    public MarkCompleteEventUpdateDTOValidator()
    {
        RuleFor(e => e.EventId).NotEmpty();
        RuleFor(e => e.Executors).Must(e => e.Distinct().Count() == e.Count());
        RuleFor(e => e.Date).NotEmpty();
        RuleFor(e => e.Count)
            .Must(e => Mark.IsValidCount(e))
            .WithMessage($"Количество должно быть больше нуля или кратное '{Mark.CountMultiplicityNumber}'.");
    }
}