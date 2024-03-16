using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ProjectAddDTOValidator : AbstractValidator<ProjectAddDTO>
{
    public ProjectAddDTOValidator()
    {
        RuleFor(e => e.FactoryNumber).NotEmpty();
        RuleFor(e => e.Areas).NotEmpty().WithMessage("Необходимо выбрать хотя бы один участок.").Must(e => e.Distinct().Count() == e.Count()).WithMessage("Участки не могут повторяться.");
    }
}