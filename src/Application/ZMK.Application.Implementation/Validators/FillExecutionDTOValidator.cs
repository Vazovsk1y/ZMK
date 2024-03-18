using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class FillExecutionDTOValidator : AbstractValidator<FillExecutionDTO>
{
    public FillExecutionDTOValidator(IValidator<AreaExecutionDTO> validator)
    {
        RuleFor(e => e.MarkId).NotEmpty();
        RuleFor(e => e.AreasExecutions)
            .NotEmpty()
            .Must(e => e.DistinctBy(i => i.AreaId).Count() == e.Count())
            .WithMessage("Среди участков не должно быть повторов.");

        RuleForEach(e => e.AreasExecutions).SetValidator(validator);
    }
}
