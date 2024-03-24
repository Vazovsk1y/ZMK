using ClosedXML.Report;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Constants;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.Domain.Common;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class MarkReportService : BaseService, IMarkReportService
{
    private const string CommonReportResourceName = "ZMK.Application.Implementation.Templates.commonMarkEventsReportTemplate.xlsx";

    private const string ModifyReportResourceName = "ZMK.Application.Implementation.Templates.modifyMarkEventsReportTemplate.xlsx";

    private const string CompleteReportResourceName = "ZMK.Application.Implementation.Templates.completeMarkEventsReportTemplate.xlsx";
    public MarkReportService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        ICurrentSessionProvider currentSessionProvider,
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result> ExportEventsToExcelAsync(ExportToExcelMarkEventsDTO dTO, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var validationResult = Validate(dTO);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        var isAbleResult = await IsAuthenticated(cancellationToken).ConfigureAwait(false);
        if (isAbleResult.IsFailure)
        {
            return isAbleResult;
        }

        var targetMark = await _dbContext.Marks.SingleOrDefaultAsync(e => e.Id == dTO.MarkId, cancellationToken);
        if (targetMark is null)
        {
            return Result.Failure(Errors.NotFound("Марка"));
        }

        _logger.LogInformation("Запрос на экспорт событий марки '{markId}' в Excel. Тип отчета '{reportType}'.", dTO.MarkId, dTO.ReportType.ToString());

        Stream stream;
        object report;
        XLTemplate template;
        switch (dTO.ReportType)
        {
            case MarkEventsReportTypes.Common:
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CommonReportResourceName) 
                        ?? throw new InvalidOperationException("Шаблон не найден.");
                    template = new XLTemplate(stream);

                    var events = await _dbContext
                       .MarksEvents
                       .Include(e => e.Mark)
                       .Include(e => ((MarkCompleteEvent)e).Area)
                       .Include(e => e.Creator)
                       .ThenInclude(e => e.Employee)
                       .AsNoTracking()
                       .Where(e => e.MarkId == dTO.MarkId)
                       .OrderByDescending(e => e.CreatedDate)
                       .Select(e => e.ToCommon())
                       .ToListAsync(cancellationToken);

                    report = targetMark.ToReport(events);
                    break;
                }
            case MarkEventsReportTypes.Modify:
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ModifyReportResourceName)
                        ?? throw new InvalidOperationException("Шаблон не найден.");
                    template = new XLTemplate(stream);

                    var events = await _dbContext
                       .MarksEvents
                       .Include(e => e.Mark)
                       .Include(e => ((MarkCompleteEvent)e).Area)
                       .Include(e => e.Creator)
                       .ThenInclude(e => e.Employee)
                       .AsNoTracking()
                       .Where(e => e.MarkId == dTO.MarkId && e.EventType == EventType.Create || e.EventType == EventType.Modify)
                       .OrderByDescending(e => e.CreatedDate)
                       .Select(e => e.ToModifyOrCreate())
                       .ToListAsync(cancellationToken);

                    report = targetMark.ToReport(events);
                    break;
                }
            case MarkEventsReportTypes.Complete:
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CompleteReportResourceName)
                        ?? throw new InvalidOperationException("Шаблон не найден.");
                    template = new XLTemplate(stream);

                    var events = await _dbContext
                       .MarkCompleteEvents
                       .Include(e => e.Area)
                       .Include(e => e.Creator)
                       .ThenInclude(e => e.Employee)
                       .Include(e => e.Executors)
                       .ThenInclude(e => e.Employee)
                       .AsNoTracking()
                       .Where(e => e.MarkId == dTO.MarkId)
                       .OrderByDescending(e => e.CreatedDate)
                       .Select(e => e.ToComplete())
                       .ToListAsync(cancellationToken);

                    report = targetMark.ToReport(events);
                    break;
                }
            default: throw new KeyNotFoundException();
        }

        template.AddVariable(report);
        template.Generate();
        template.Workbook
            .Worksheet(template.Workbook.Worksheets.First().Name)
            .ColumnsUsed()
            .AdjustToContents();

        template.SaveAs(dTO.FilePath);
        _logger.LogInformation("События были успешно экспортированы в Excel.");
        return Result.Success();
    }
}

public record MarkEventsReport<T>(
    DateTime CreatedDate,
    string MarkTitle,
    double MarkCount,
    string MarkCode,
    double MarkWeight,
    IEnumerable<T> Events);

public record CommonReportMarkEvent(DateTime Date, double Count, string Title, string EventType, string Creator, string? Remark);

public record ModifyOrCreateReportMarkEvent(DateTime Date, double MarkCount, string MarkCode, double MarkWeight, string MarkTitle, string EventType, string Creator, string? Remark);

public record CompleteReportMarkEvent(DateTime CompleteDate, double CompleteCount, string AreaTitle, string EventType, string Executors, string Creator, string? Remark);