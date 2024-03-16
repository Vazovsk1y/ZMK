using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ProjectSettingsUpdateDTOValidator : AbstractValidator<ProjectSettingsUpdateDTO>
{
    public ProjectSettingsUpdateDTOValidator()
    {
        RuleFor(e => e.ProjectId).NotEmpty();
        RuleFor(e => e.Areas).NotEmpty().Must(e => e.Distinct().Count() == e.Count()).WithMessage("Участки не могут повторяться.");
    }
}