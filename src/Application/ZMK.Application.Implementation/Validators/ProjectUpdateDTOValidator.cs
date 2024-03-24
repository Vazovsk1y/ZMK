using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ProjectUpdateDTOValidator : AbstractValidator<ProjectUpdateDTO>
{
    public ProjectUpdateDTOValidator()
    {
        RuleFor(e => e.ProjectId).NotEmpty();
        RuleFor(e => e.FactoryNumber).NotEmpty();
    }
}