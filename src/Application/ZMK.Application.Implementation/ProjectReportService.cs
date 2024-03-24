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
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;

namespace ZMK.Application.Implementation;

public class ProjectReportService : BaseService, IProjectReportService
{
    private const string ByAreasReportResourceName = "ZMK.Application.Implementation.Templates.byAreasProjectExecutionReportTemplate.xlsx";

    private const string ByExecutorsReportResourceName = "ZMK.Application.Implementation.Templates.byExecutorsProjectExecutionReportTemplate.xlsx";

    public ProjectReportService(
        IClock clock,
        ILogger<BaseService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ZMKDbContext dbContext,
        ICurrentSessionProvider currentSessionProvider,
        UserManager<User> userManager) : base(clock, logger, serviceScopeFactory, dbContext, currentSessionProvider, userManager)
    {
    }

    public async Task<Result> ExportExecutionToExcelAsync(ExportToExcelProjectExecutionDTO dTO, CancellationToken cancellationToken = default)
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

        var targetProject = await _dbContext
            .Projects
            .Include(e => e.Creator)
            .ThenInclude(e => e.Employee)
            .SingleOrDefaultAsync(e => e.Id == dTO.ProjectId, cancellationToken);

        if (targetProject is null)
        {
            return Result.Failure(Errors.NotFound("Проект"));
        }

        _logger.LogInformation("Запрос на экспорт выполнения проекта {projectId} в Excel. Тип отчета '{reportType}'.", dTO.ProjectId, dTO.ReportType.ToString());
        Stream stream;
        object report;
        XLTemplate template;

        switch (dTO.ReportType)
        {
            case ProjectExecutionReportTypes.ByAreas:
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ByAreasReportResourceName)
                        ?? throw new InvalidOperationException("Шаблон не найден.");
                    template = new XLTemplate(stream);

                    var items = await ByAreas(targetProject.Id, dTO.Range);
                    report = targetProject.ToReport(dTO, items);
                    break;
                }
            case ProjectExecutionReportTypes.ByExecutors:
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ByExecutorsReportResourceName)
                        ?? throw new InvalidOperationException("Шаблон не найден.");
                    template = new XLTemplate(stream);

                    var items = await ByExecutors(targetProject.Id, dTO.Range);
                    report = targetProject.ToReport(dTO, items);
                    break;
                }
            default:
                throw new NotImplementedException();
        }

        template.AddVariable(report);
        template.Generate();
        template.Workbook.Worksheets
            .Worksheet(template.Workbook.Worksheets.First().Name)
            .ColumnsUsed()
            .AdjustToContents();

        template.SaveAs(dTO.FilePath);
        _logger.LogInformation("Отчет успешно успешно экспортирован в Excel.");
        return Result.Success();
    }

    private async Task<IEnumerable<ByAreaRow>> ByAreas(Guid projectId, Contracts.Range? range)
    {
        var projectAreas = _dbContext
            .ProjectsAreas
            .Where(e => e.ProjectId == projectId)
            .Select(e => e.AreaId);

        var query = _dbContext
            .MarkCompleteEvents
            .Include(e => e.Mark)
            .Include(e => e.Area)
            .AsNoTracking();

        if (range is not null)
        {
            query = query.Where(e => e.CompleteDate.Date >= range.From.Date && e.CompleteDate.Date <= range.To.Date);
        }

        var events = await query
            .Where(e => projectAreas.Contains(e.AreaId) && e.Mark.ProjectId == projectId)
            .GroupBy(e => e.Area)
            .ToListAsync();

        var projectMarks = await _dbContext
            .Marks
            .AsNoTracking()
            .Where(e => e.ProjectId == projectId)
            .ToListAsync();

        return events.Select(e =>
        {
            double completeCount = e.Sum(e => e.CompleteCount);
            double leftCount = projectMarks.Sum(e => e.Count) - completeCount;
            double completeWeight = e.Sum(e => e.CompleteCount * e.Mark.Weight);
            double leftWeight = projectMarks.Sum(e => e.Count * e.Weight) - completeWeight;

            return new ByAreaRow
            (
                e.Key.Title,
                completeCount,
                completeWeight.RoundForReport(),
                leftCount,
                leftWeight.RoundForReport()
            );
        });
    }

    private async Task<IEnumerable<ByExecutorRow>> ByExecutors(Guid projectId, Contracts.Range? range)
    {
        var projectAreas = _dbContext
            .ProjectsAreas
            .Where(e => e.ProjectId == projectId)
            .Select(e => e.AreaId);

        var query = _dbContext
            .MarkCompleteEvents
            .Include(e => e.Area)
            .Include(e => e.Mark)
            .Include(e => e.Executors)
            .ThenInclude(e => e.Employee)
            .AsNoTracking();

        if (range is not null)
        {
            query = query.Where(e => e.CompleteDate.Date >= range.From.Date && e.CompleteDate.Date <= range.To.Date);
        }

        var events = await query
            .Where(e => e.Mark.ProjectId == projectId && e.Executors.Count() > 0 && projectAreas.Contains(e.AreaId))
            .ToListAsync();

        var report = events
            .SelectMany(e => e.Executors.Select(exec => new
            {
                Area = new { e.Area.Id, e.Area.Title },
                Executor = new { exec.Employee.Id, exec.Employee.FullName },
                CompleteCount = Math.Round(e.CompleteCount / e.Executors.Count()),
                CompleteWeight = Math.Round(e.CompleteCount / e.Executors.Count()) * e.Mark.Weight
            }))
            .GroupBy(e => new { e.Area, e.Executor })
            .Select(group => new ByExecutorRow
            (
                group.Key.Area.Title,
                group.Key.Executor.FullName,
                group.Sum(e => e.CompleteCount),
                group.Sum(e => e.CompleteWeight).RoundForReport()
            ))
            .ToList();

        return report;
    }
}

public record ProjectExecutionReport<T>(
    DateTime CreatedDate,
    string ProjectFactoryNumber,
    DateTimeOffset ProjectCreatedDate,
    string ProjectCreator,
    string ReportType,
    string ReportRange,
    IEnumerable<T> Items
    );

public record ByAreaRow(string AreaTitle, double CompleteCount, double CompleteWeight, double LeftCount, double LeftWeight);

public record ByExecutorRow(string AreaTitle, string Executor, double CompleteCount, double CompleteWeight);