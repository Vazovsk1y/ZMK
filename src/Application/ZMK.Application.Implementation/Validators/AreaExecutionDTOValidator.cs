using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class AreaExecutionDTOValidator : AbstractValidator<AreaExecutionDTO>
{
    public const double MultiplicityNumber = 0.5d;
    public AreaExecutionDTOValidator()
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.Count)
            .Must(e => (e > 0 && e % 1 == 0) || (e > 0 && e % MultiplicityNumber == 0))
            .WithMessage($"Количество должно быть больше нуля или кратное '{MultiplicityNumber}'.");

        RuleFor(e => e.Executors).Must(e => e.Distinct().Count() == e.Count()).WithMessage("Среди исполнителей не должно быть повторов.");
    }
}