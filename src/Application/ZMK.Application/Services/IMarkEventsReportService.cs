using ZMK.Application.Contracts;
using ZMK.Domain.Shared;

namespace ZMK.Application.Services;

public interface IMarkEventsReportService
{
    Task<Result> ExportToExcelAsync(ExportToExcelMarkEventsDTO dTO, CancellationToken cancellationToken = default);
}