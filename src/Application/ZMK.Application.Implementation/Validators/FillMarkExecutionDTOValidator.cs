using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class FillMarkExecutionDTOValidator : AbstractValidator<FillMarkExecutionDTO>
{
    public FillMarkExecutionDTOValidator(IValidator<AreaExecutionDTO> validator)
    {
        RuleFor(e => e.MarkId).NotEmpty();
        RuleFor(e => e.Executions)
            .NotEmpty()
            .Must(e => e.DistinctBy(i => i.AreaId).Count() == e.Count())
            .WithMessage("Среди участков не должно быть повторов.");

        RuleForEach(e => e.Executions).SetValidator(validator);
    }
}
