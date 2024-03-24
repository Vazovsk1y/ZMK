using FluentValidation;
using ZMK.Application.Contracts;

namespace ZMK.Application.Implementation.Validators;

public class ExportToExcelProjectExecutionDTOValidator : AbstractValidator<ExportToExcelProjectExecutionDTO>
{
    public ExportToExcelProjectExecutionDTOValidator()
    {
        RuleFor(e => e.ProjectId).NotEmpty();
        RuleFor(e => e.FilePath).NotEmpty().Must(e => e.EndsWith(Constants.Common.XlsxExtension, StringComparison.OrdinalIgnoreCase));
    }
}