using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Entities;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class MarksPanelViewModel : TitledViewModel,
        IRecipient<MarksAddedMessage>, IRecipient<MarkExecutionFilledMessage>
{
    public const string ByKg = "В Килограммах";
    public const string ByUnits = "В штуках";
    public const string ByPercents = "В процентах";

    public ObservableCollection<MarkViewModel> Marks { get; } = [];

    public ObservableCollection<AreaInfo> AvailableAreas { get; } = [];

    public ObservableCollection<ExecutorInfo> AvailableExecutors { get; } = [];

    public ProjectViewModel SelectedProject { get; }

    public Dictionary<Guid, Dictionary<Guid, double>> Executions { get; private set; } = [];

    public ObservableCollection<string> DisplayExecutionInOptions { get; } = [ByUnits, ByKg, ByPercents];

    public ObservableCollection<string> EventTypesOptions { get; } = [MarkEventViewModel.CreateEventType, MarkEventViewModel.ModifyEventType, MarkEventViewModel.CompleteEventType, MarkEventViewModel.CommonEventType];

    public Dictionary<Guid, ObservableCollection<MarkEventViewModel>> MarkEventsCache { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCompleteMarkEventsChangesCommand))]
    private ObservableCollection<MarkEventViewModel>? _selectedMarkEvents;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(FillExecutionCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportEventsToExcelCommand))]
    private MarkViewModel? _selectedMark;

    private string _selectedDisplayInOption = null!;

    public string SelectedDisplayInOption
    {
        get => _selectedDisplayInOption;
        set
        {
            if (value is not null && SetProperty(ref _selectedDisplayInOption, value))
            {
                CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
            }
        }
    }

    private string _selectedEventTypeOption = MarkEventViewModel.CommonEventType;

    public string SelectedEventTypeOption
    {
        get => _selectedEventTypeOption;
        set
        {
            SetProperty(ref _selectedEventTypeOption, value);
            if (!string.IsNullOrWhiteSpace(value))
            {
                RefreshMarkEventsForSelectedMark(value);
            }
            SaveCompleteMarkEventsChangesCommand.NotifyCanExecuteChanged();
            ExportEventsToExcelCommand.NotifyCanExecuteChanged();
        }
    }

    private AreaInfo? _selectedArea;

    public AreaInfo? SelectedArea
    {
        get => _selectedArea;
        set
        {
            if (value is not null && SetProperty(ref _selectedArea, value))
            {
                CalculateExecutionForEachMark(value.Id, SelectedDisplayInOption);
            }
        }
    }

    public double TotalCount => Marks.Sum(e => e.Count).RoundForDisplay();

    public double TotalWeight => Marks.Sum(e => e.TotalWeight).RoundForDisplay();

    public double TotalComplete => CalculateTotalComplete().RoundForDisplay();

    public double TotalLeft => CalculateTotalLeft().RoundForDisplay();

    public MarksPanelViewModel(ProjectsPanelViewModel projectsPanelViewModel)
    {
        ArgumentNullException.ThrowIfNull(projectsPanelViewModel.SelectedProject);
        SelectedProject = projectsPanelViewModel.SelectedProject;
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете удалить марку '{SelectedMark!.Title} - {SelectedMark!.Code}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await markService.DeleteAsync(SelectedMark.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                MarkEventsCache.Remove(SelectedMark.Id);
                App.Current.Dispatcher.Invoke(() =>
                {
                    SelectedMarkEvents = null;
                    Marks.Remove(SelectedMark);
                });
                NotifyTotalProperties();
                MessageBoxHelper.ShowInfoBox("Марка успешно удалена.");
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() => SelectedMark is not null;

    [RelayCommand]
    public async Task MarkSelectionChanged(object param)
    {
        if (param is not MarkViewModel mark)
        {
            SelectedMark = null;
            return;
        }

        IsEnabled = false;

        if (MarkEventsCache.TryGetValue(mark.Id, out var _))
        {
            SelectedMark = mark;
            SelectedEventTypeOption = string.IsNullOrWhiteSpace(SelectedEventTypeOption) ? MarkEventViewModel.CommonEventType : SelectedEventTypeOption;
            IsEnabled = true;
            return;
        }

        await RefreshCacheMarkEventsFor(mark.Id);
        SelectedMark = mark;
        SelectedEventTypeOption = string.IsNullOrWhiteSpace(SelectedEventTypeOption) ? MarkEventViewModel.CommonEventType : SelectedEventTypeOption;
        IsEnabled = true;
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<MarkAddWindow>();
    }

    [RelayCommand]
    public async Task ImportFromExcel()
    {
        if (ImportFromExcelCommand.IsRunning || SelectedProject is null)
        {
            return;
        }

        const string Filter = "Excel Files (*.xlsx; *.xls)|*.xlsx; *.xls";
        const string Title = "Выберите файл:";
        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = false,
            Filter = Filter,
            Title = Title,
            RestoreDirectory = true,
        };

        var dialogResult = fileDialog.ShowDialog();

        string selectedFile;
        if (dialogResult is bool dresult && dresult is true)
        {
            selectedFile = fileDialog.FileName;
        }
        else
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var result = await markService.AddFromXlsxAsync(selectedFile, SelectedProject.Id);
        if (result.IsSuccess)
        {
            var addedMarks = await dbContext
            .Marks
            .AsNoTracking()
            .Where(e => result.Value.Contains(e.Id))
            .Select(e => e.ToViewModel())
            .ToListAsync()
            .ConfigureAwait(false);

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Marks.AddRange(addedMarks);
                CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
                MessageBoxHelper.ShowInfoBox($"{result.Value.Count} марок было успешно добавлено.");
            });
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        var modifiedMarks = Marks.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedMarks.Count == 0)
        {
            return;
        }

        IsEnabled = false;
        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();

        var results = new List<Result>();
        List<Guid> updatedMarksIds = [];
        foreach (var mark in modifiedMarks)
        {
            var dto = new MarkUpdateDTO(mark.Id, mark.Code, mark.Title, mark.Order, mark.Weight, mark.Count);
            var updateResult = await markService.UpdateAsync(dto);

            if (updateResult.IsFailure)
            {
                mark.RollBackChanges();
            }
            else
            {
                updatedMarksIds.Add(mark.Id);
                mark.SaveState();
            }

            results.Add(updateResult);
        }

        foreach (var id in updatedMarksIds)
        {
            await RefreshCacheMarkEventsFor(id);
        }

        if (SelectedMark is not null && updatedMarksIds.Contains(SelectedMark.Id))
        {
            RefreshMarkEventsForSelectedMark(SelectedEventTypeOption);
        }

        if (updatedMarksIds.Count > 0)
        {
            CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
        }

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;
    }

    [RelayCommand]
    public void RollbackChanges()
    {
        var modifiedMarks = Marks.Where(e => e.IsModified()).ToList();
        if (modifiedMarks.Count == 0)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            modifiedMarks.ForEach(e => e.RollBackChanges());
        }
    }

    [RelayCommand(CanExecute = nameof(CanFillExecution))]
    public void FillExecution()
    {
        using var scope = App.Services.CreateScope();
        var viewModel = scope.ServiceProvider.GetRequiredService<MarkFillExecutionWindowViewModel>();
        viewModel.SelectedMark = SelectedMark!;
        viewModel.IsActive = true;

        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<MarkFillExecutionWindow, MarkFillExecutionWindowViewModel>(viewModel);
    }

    public bool CanFillExecution() => SelectedMark is not null;

    [RelayCommand(CanExecute = nameof(CanSaveMarkEventsChanges))]
    public async Task SaveCompleteMarkEventsChanges()
    {
        var modifiedEvents = SelectedMarkEvents!.Where(e => e is MarkCompleteEventViewModel ce && ce.IsModified()).Cast<MarkCompleteEventViewModel>().ToList();
        if (SaveChangesCommand.IsRunning || modifiedEvents.Count == 0)
        {
            return;
        }

        IsEnabled = false;
        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();

        var results = new List<Result>();
        foreach (var @event in modifiedEvents)
        {
            var dto = new MarkCompleteEventUpdateDTO(@event.Id, @event.Area.Id, @event.Date, @event.MarkCount, @event.Executors.Select(e => e.Id).ToArray(), @event.Remark);
            var updateResult = await markService.UpdateCompleteEventAsync(dto);

            if (updateResult.IsFailure)
            {
                @event.RollBackChanges();
            }
            else
            {
                @event.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Where(e => e.IsSuccess).Any())
        {
            await RefreshExecutionForSelectedMark();
            CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
        }

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;
    }

    public bool CanSaveMarkEventsChanges() =>
        (SelectedEventTypeOption == MarkEventViewModel.CommonEventType
        || SelectedEventTypeOption == MarkEventViewModel.CompleteEventType) && SelectedMarkEvents is ICollection<MarkEventViewModel> { Count: > 0 } e && e.Any(e => e.EventType == MarkEventViewModel.CompleteEventType);

    [RelayCommand(CanExecute = nameof(CanExportEventsToExcel))]
    public async Task ExportEventsToExcel()
    {
        if (ExportEventsToExcelCommand.IsRunning)
        {
            return;
        }

        IsEnabled = false;

        const string Filter = "Excel Files (*.xlsx)|*.xlsx";
        const string Title = "Выберите файл:";
        var fileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = Filter,
            Title = Title,
            RestoreDirectory = true,
        };

        var dialogResult = fileDialog.ShowDialog();

        string selectedFilePath;
        if (dialogResult is bool dresult && dresult is true && !string.IsNullOrWhiteSpace(fileDialog.FileName))
        {
            selectedFilePath = fileDialog.FileName;
        }
        else
        {
            IsEnabled = true;
            return;
        }

        using var scope = App.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IMarkEventsReportService>();

        var dto = new ExportToExcelMarkEventsDTO(SelectedMark!.Id, SelectedEventTypeOption!.ToReportType(), selectedFilePath);
        var result = await service.ExportToExcelAsync(dto);
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

    public bool CanExportEventsToExcel() => SelectedMark is not null && SelectedEventTypeOption != MarkEventViewModel.CreateEventType;

    public void Receive(MarksAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Marks.AddRange(message.Marks);
            CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
            MessageBoxHelper.ShowInfoBox("Марки были успешно добавлены.");
        });
    }

    public void Receive(MarkExecutionFilledMessage message)
    {
        App.Current.Dispatcher.Invoke(async () =>
        {
            foreach (var item in message.AreasCounts)
            {
                var data = Executions[item.Key];
                data.TryGetValue(message.MarkId, out double previousCompleteCount);
                data[message.MarkId] = previousCompleteCount + item.Value;
            }

            await RefreshCacheMarkEventsFor(message.MarkId);
            RefreshMarkEventsForSelectedMark(SelectedEventTypeOption);

            CalculateExecutionForEachMark(SelectedArea?.Id, SelectedDisplayInOption);
            MessageBoxHelper.ShowInfoBox("Выполнение марки успешно сохранено.");
        });
    }

    protected override async void OnActivated()
    {
        base.OnActivated();

        IsEnabled = false;

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var marks = await dbContext
            .Marks
            .AsNoTracking()
            .Where(e => e.ProjectId == SelectedProject.Id)
            .OrderBy(e => e.Order)
            .ThenBy(e => e.Title)
            .Select(e => e.ToViewModel())
            .ToListAsync()
            .ConfigureAwait(false);

        var areas = await dbContext
            .Areas
            .AsNoTracking()
            .OrderBy(e => e.Order)
            .Select(e => new AreaInfo(e.Id, e.Title))
            .ToListAsync();

        var completeEvents = await dbContext
            .MarkCompleteEvents
            .AsNoTracking()
            .Where(e => marks.Select(e => e.Id).Contains(e.MarkId))
            .ToListAsync();

        var executors = await dbContext
            .Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => new ExecutorInfo(e.Id, string.IsNullOrWhiteSpace(e.Post) ? e.FullName : $"{e.FullName} ({e.Post})"))
            .ToListAsync();

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Executions = areas.ToDictionary(
                e => e.Id,
                e => completeEvents.Where(i => i.AreaId == e.Id).GroupBy(e => e.MarkId).ToDictionary(e => e.Key, e => e.Sum(e => e.CompleteCount)));
            Marks.AddRange(marks);
            AvailableAreas.AddRange(areas);
            AvailableExecutors.AddRange(executors);

            SelectedArea = AvailableAreas.FirstOrDefault();
            SelectedDisplayInOption = ByUnits;
            IsEnabled = true;
        });
    }

    private void CalculateExecutionForEachMark(Guid? areaId, string displayInOption)
    {
        if (areaId is null)
        {
            return;
        }

        bool isExists = Executions.TryGetValue((Guid)areaId, out var data);
        if (!isExists || data is null)
        {
            return;
        }

        IsEnabled = false;
        foreach (var item in Marks.Where(e => !e.IsModified()))
        {
            data.TryGetValue(item.Id, out double completeCount);

            switch (displayInOption)
            {
                case string when ByUnits == displayInOption:
                    {
                        item.Complete = completeCount;
                        item.Left = item.Count - completeCount;
                        break;
                    }
                case string when ByKg == displayInOption:
                    {
                        double completeInKg = (completeCount * item.Weight).RoundForDisplay();
                        item.Complete = completeInKg;
                        item.Left = ((item.Count - completeCount) * item.Weight).RoundForDisplay();
                        break;
                    }
                case string when ByPercents == displayInOption:
                    {
                        double completeInPercents = (item.Count == 0 ? 0 : (completeCount * 100) / item.Count).RoundForDisplay();
                        item.Complete = completeInPercents;
                        item.Left = (100 - completeInPercents).RoundForDisplay();
                        break;
                    }
                default:
                    {
                        item.Complete = completeCount;
                        item.Left = item.Count - completeCount;
                        break;
                    }
            }
        }

        NotifyTotalProperties();
        IsEnabled = true;
    }

    private void NotifyTotalProperties()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(TotalWeight));
        OnPropertyChanged(nameof(TotalComplete));
        OnPropertyChanged(nameof(TotalLeft));
    }

    private double CalculateTotalComplete()
    {
        if (SelectedDisplayInOption == ByPercents)
        {
            return CalculateTotalCompleteInPercents();
        }

        return Marks.Sum(e => e.Complete).Round(2);
    }

    private double CalculateTotalCompleteInPercents()
    {
        if (SelectedArea is null)
        {
            return 0;
        }

        bool isExists = Executions.TryGetValue(SelectedArea.Id, out var data);
        double totalCount = TotalCount;
        if (!isExists || data is null || totalCount == 0)
        {
            return 0;
        }

        double totalCompleteCount = 0;
        foreach (var item in Marks)
        {
            data.TryGetValue(item.Id, out double completeCount);
            totalCompleteCount += completeCount;
        }

        return totalCompleteCount * 100 / totalCount;
    }

    private double CalculateTotalLeft()
    {
        return SelectedDisplayInOption == ByPercents ?
            (100 - CalculateTotalCompleteInPercents()) is double n && n == 100 && Marks.Count == 0 ? 0 : n
            :
            Marks.Sum(e => e.Left);
    }

    private async Task RefreshCacheMarkEventsFor(Guid markId)
    {
        IsEnabled = false;
        var events = await GetEventsFor(markId);
        MarkEventsCache[markId] = events;
        IsEnabled = true;
    }

    private static async Task<ObservableCollection<MarkEventViewModel>> GetEventsFor(Guid markId)
    {
        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var events = new ObservableCollection<MarkEventViewModel>(await dbContext
            .MarksEvents
            .Include(e => e.Mark)
            .Include(e => ((MarkCompleteEvent)e).Area)
            .Include(e => ((MarkCompleteEvent)e).Executors)
            .ThenInclude(e => e.Employee)
            .Include(e => e.Creator)
            .ThenInclude(e => e.Employee)
            .AsNoTracking()
            .Where(e => e.MarkId == markId)
            .OrderByDescending(e => e.CreatedDate)
            .Select(e => e.ToViewModel())
            .ToListAsync());

        return events;
    }

    private void RefreshMarkEventsForSelectedMark(string eventType)
    {
        if (SelectedMark is null || !MarkEventsCache.TryGetValue(SelectedMark.Id, out var data))
        {
            return;
        }

        foreach (var item in data)
        {
            item.IsEditable = item.EventType == MarkEventViewModel.CompleteEventType && eventType != MarkEventViewModel.CommonEventType;
            item.DisplayEventType = eventType;
        }

        if (eventType == MarkEventViewModel.CommonEventType)
        {
            SelectedMarkEvents = data;
        }
        else
        {
            SelectedMarkEvents = new(data.Where(e => e.EventType == eventType));
        }
    }

    private async Task RefreshExecutionForSelectedMark()
    {
        if (SelectedMark is null)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var completeEvents = await dbContext
            .MarkCompleteEvents
            .AsNoTracking()
            .Where(e => SelectedMark.Id == e.MarkId)
            .ToListAsync();

        foreach (var areaId in Executions.Keys)
        {
            var data = Executions[areaId];
            data[SelectedMark.Id] = completeEvents.Where(e => e.MarkId == SelectedMark.Id && e.AreaId == areaId).Sum(e => e.CompleteCount);
        }
    }
}

public record ExecutorInfo(Guid Id, string FullNameAndPost);

public record AreaInfo(Guid Id, string Title);