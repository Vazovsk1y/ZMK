using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Project;

public partial class ProjectReportsViewModel : TitledViewModel
{
    public ProjectViewModel SelectedProject { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateReportCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportToExcelCommand))]
    private string _selectedReportByOption = ByAreaViewModel.ByAreasOption;

    public ObservableCollection<string> ReportByOptions { get; } = [ByAreaViewModel.ByAreasOption, ByExecutorViewModel.ByExecutorsOption];

    [ObservableProperty]
    public ObservableCollection<ObservableObject>? _report;

    [ObservableProperty]
    private DateTime _from = new(2024, 1, 1);

    [ObservableProperty]
    private DateTime _to = DateTime.Now;

    [ObservableProperty]
    private bool _withoutRange = true;

    public ProjectReportsViewModel(ProjectsPanelViewModel projectsPanelViewModel)
    {
        ArgumentNullException.ThrowIfNull(projectsPanelViewModel.SelectedProject);
        SelectedProject = projectsPanelViewModel.SelectedProject;
        ControlTitle = "Окно отчетов выбранного проекта";
    }

    [RelayCommand(CanExecute = nameof(CanGenerateReport))]
    public async Task GenerateReport()
    {
        if (GenerateReportCommand.IsRunning)
        {
            return;
        }

        IsEnabled = false;
        switch (SelectedReportByOption)
        {
            case ByAreaViewModel.ByAreasOption:
                {
                    Report = new(await ByAreas());
                    break;
                }
            case ByExecutorViewModel.ByExecutorsOption:
                {
                    Report = new(await ByExecutors());
                    break;
                }
            default:
                throw new KeyNotFoundException();
        }
        IsEnabled = true;
    }

    public bool CanGenerateReport() => !string.IsNullOrWhiteSpace(SelectedReportByOption);

    [RelayCommand]
    public async Task ExportToExcel()
    {
        string? selectedFilePath = DialogHelper.ShowSaveXlsxFileDialog();
        if (string.IsNullOrWhiteSpace(selectedFilePath) || ExportToExcelCommand.IsRunning)
        {
            return;
        }

        IsEnabled = false;
        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IProjectReportService>();

        var dto = new ExportToExcelProjectExecutionDTO(SelectedProject.Id, SelectedReportByOption!.ToProjectReportType(), selectedFilePath, WithoutRange ? null : new Application.Contracts.Range(From, To));
        var result = await service.ExportExecutionToExcelAsync(dto);
        if (result.IsSuccess)
        {
            MessageBoxHelper.ShowInfoBox("Отчет был успешно экспортирован.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
        IsEnabled = true;
    }

    public bool CanExportToExcel() => !string.IsNullOrWhiteSpace(SelectedReportByOption);

    private async Task<IEnumerable<ByAreaViewModel>> ByAreas()
    {
        ArgumentNullException.ThrowIfNull(SelectedProject);

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var projectAreas = dbContext
            .ProjectsAreas
            .Where(e => e.ProjectId == SelectedProject.Id)
            .Select(e => e.AreaId);

        var query = dbContext
            .MarkCompleteEvents
            .Include(e => e.Mark)
            .Include(e => e.Area)
            .AsNoTracking();

        if (!WithoutRange)
        {
            query = query.Where(e => e.CompleteDate.Date >= From.Date && e.CompleteDate.Date <= To.Date);
        }

        var events = await query
            .Where(e => projectAreas.Contains(e.AreaId) && e.Mark.ProjectId == SelectedProject.Id)
            .GroupBy(e => e.Area)
            .ToListAsync();

        var projectMarks = await dbContext
            .Marks
            .AsNoTracking()
            .Where(e => e.ProjectId == SelectedProject.Id)
            .ToListAsync();

        return events.Select(e =>
        {
            double completeCount = e.Sum(e => e.CompleteCount);
            double leftCount = projectMarks.Sum(e => e.Count) - completeCount;
            double completeWeight = e.Sum(e => e.CompleteCount * e.Mark.Weight);
            double leftWeight = projectMarks.Sum(e => e.Count * e.Weight) - completeWeight;

            return new ByAreaViewModel
            {
                Area = new AreaInfo(e.Key.Id, e.Key.Title),
                CompleteCount = completeCount,
                LeftCount = leftCount,
                LeftWeight = leftWeight.RoundForReport(),
                CompleteWeight = completeWeight.RoundForReport()
            };
        });
    }

    private async Task<IEnumerable<ByExecutorViewModel>> ByExecutors()
    {
        ArgumentNullException.ThrowIfNull(SelectedProject);

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var projectAreas = dbContext.ProjectsAreas.Where(e => e.ProjectId == SelectedProject.Id).Select(e => e.AreaId);

        var query = dbContext
            .MarkCompleteEvents
            .Include(e => e.Area)
            .Include(e => e.Mark)
            .Include(e => e.Executors)
            .ThenInclude(e => e.Employee)
            .AsNoTracking();

        if (!WithoutRange)
        {
            query = query.Where(e => e.CompleteDate.Date >= From.Date && e.CompleteDate.Date <= To.Date);
        }

        var events = await query
            .Where(e => e.Mark.ProjectId == SelectedProject.Id && e.Executors.Count() > 0 && projectAreas.Contains(e.AreaId))
            .ToListAsync();

        var report = events
            .SelectMany(e => e.Executors.Select(exec => new
            {
                Area = new AreaInfo(e.Area.Id, e.Area.Title),
                Executor = exec.Employee.ToInfo(),
                CompleteCount = Math.Round(e.CompleteCount / e.Executors.Count()),
                CompleteWeight = Math.Round(e.CompleteCount / e.Executors.Count()) * e.Mark.Weight
            }))
            .GroupBy(e => new { e.Area, e.Executor })
            .Select(group => new ByExecutorViewModel
            {
                Area = group.Key.Area,
                Executor = group.Key.Executor,
                CompleteCount = group.Sum(e => e.CompleteCount),
                CompleteWeight = group.Sum(e => e.CompleteWeight).RoundForReport(),
            })
            .ToList();

        return report;
    }
}

public class ByExecutorViewModel : ObservableObject
{
    public const string ByExecutorsOption = "По исполнителям";

    public required AreaInfo Area { get; init; }

    public required ExecutorInfo Executor { get; init; }

    public required double CompleteCount { get; init; }

    public required double CompleteWeight { get; init; }
}

public class ByAreaViewModel : ObservableObject
{
    public const string ByAreasOption = "По участкам";

    public required AreaInfo Area { get; init; }

    public required double CompleteCount { get; init; }

    public required double CompleteWeight { get; init; }

    public required double LeftCount { get; init; }

    public required double LeftWeight { get; init; }
}