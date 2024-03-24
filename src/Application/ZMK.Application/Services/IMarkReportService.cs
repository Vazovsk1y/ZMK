using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IMarkReportService
{
    Task<Result> ExportEventsToExcelAsync(ExportToExcelMarkEventsDTO dTO, CancellationToken cancellationToken = default);
}