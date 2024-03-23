using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ExportToExcelMarkEventsDTOValidator : AbstractValidator<ExportToExcelMarkEventsDTO>
{
    public ExportToExcelMarkEventsDTOValidator()
    {
        RuleFor(e => e.MarkId).NotEmpty();
        RuleFor(e => e.FilePath).NotEmpty().Must(e => e.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));
    }
}
