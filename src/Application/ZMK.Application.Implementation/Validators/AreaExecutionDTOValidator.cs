using FluentValidation;
using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Validators;

public class AreaExecutionDTOValidator : AbstractValidator<AreaExecutionDTO>
{
    public AreaExecutionDTOValidator()
    {
        RuleFor(e => e.AreaId).NotEmpty();
        RuleFor(e => e.CompleteDate).NotEmpty();
        RuleFor(e => e.Count)
            .Must(Mark.IsValidCount)
            .WithMessage($"Количество должно быть больше нуля или кратное '{Mark.CountMultiplicityNumber}'.");

        RuleFor(e => e.Executors).Must(e => e.Distinct().Count() == e.Count()).WithMessage("Среди исполнителей не должно быть повторов.");
    }
}