using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class MarkExecutionDTOValidator : AbstractValidator<MarkExecutionDTO>
{
    public MarkExecutionDTOValidator(IValidator<AreaExecutionDTO> validator)
    {
        RuleFor(e => e.Id).NotEmpty();
        RuleFor(e => e.AreasExecutions)
            .Must(e => e.DistinctBy(i => i.Id).Count() == e.Count())
            .WithMessage("Среди участков не должно быть повторов.");

        RuleForEach(e => e.AreasExecutions).SetValidator(validator);
    }
}
