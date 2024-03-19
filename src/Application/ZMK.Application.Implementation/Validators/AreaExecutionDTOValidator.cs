using FluentValidation;
using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Validators;

public class AreaExecutionDTOValidator : AbstractValidator<AreaExecutionDTO>
{
    public AreaExecutionDTOValidator()
    {
        RuleFor(e => e.AreaId).NotEmpty();
        RuleFor(e => e.Date).NotEmpty();
        RuleFor(e => e.Count)
            .Must(e => Mark.IsValidCount(e))
            .WithMessage($"Количество должно быть больше нуля или кратное '{Mark.CountMultiplicityNumber}'.");

        RuleFor(e => e.Executors).Must(e => e.Distinct().Count() == e.Count()).WithMessage("Среди исполнителей не должно быть повторов.");
    }
}