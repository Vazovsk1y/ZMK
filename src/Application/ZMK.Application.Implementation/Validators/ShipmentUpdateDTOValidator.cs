using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ShipmentUpdateDTOValidator : AbstractValidator<ShipmentUpdateDTO>
{
    public ShipmentUpdateDTOValidator()
    {
        RuleFor(e => e.ShipmentId).NotEmpty();
        RuleFor(e => e.ShipmentDate).NotEmpty();
        RuleFor(e => e.Number).NotEmpty().WithMessage("Номер погрузки обязательное к заполнению поле.");
    }
}