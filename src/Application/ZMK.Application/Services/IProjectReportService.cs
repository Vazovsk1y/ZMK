using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IProjectReportService
{
    Task<Result> ExportExecutionToExcelAsync(ExportToExcelProjectExecutionDTO dTO, CancellationToken cancellationToken = default);
}