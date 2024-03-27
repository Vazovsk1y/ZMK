using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ShipmentAddDTOValidator : AbstractValidator<ShipmentAddDTO>
{
    public ShipmentAddDTOValidator()
    {
        RuleFor(e => e.ProjectId).NotEmpty();
        RuleFor(e => e.Number).NotEmpty().WithMessage("Номер погрузки обязательно к заполнению поле.");
    }
}