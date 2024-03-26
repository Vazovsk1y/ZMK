﻿using FluentValidation;
using ZMK.Application.Contracts;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation.Validators;

public class MarkUpdateDTOValidator : AbstractValidator<MarkUpdateDTO>
{
    public MarkUpdateDTOValidator()
    {
        RuleFor(e => e.MarkId).NotEmpty();
        RuleFor(e => e.Title).NotEmpty();
        RuleFor(e => e.Weight).NotEmpty().GreaterThanOrEqualTo(Mark.MinWeight);
        RuleFor(e => e.Code).NotEmpty();
        RuleFor(e => e.Count)
            .Must(Mark.IsValidCount)
            .WithMessage($"Количество должно быть больше нуля или кратное '{Mark.CountMultiplicityNumber}'.");
        RuleFor(e => e.Order).NotEmpty().GreaterThanOrEqualTo(Mark.MinOrder);
    }
}
