using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;

namespace ZMK.Wpf.ViewModels;

public partial class MarkFillExecutionWindowViewModel : DialogViewModel
{
    private MarkViewModel _selectedMark = null!;
    public MarkViewModel SelectedMark
    {
        get
        {
            ArgumentNullException.ThrowIfNull(_selectedMark);
            return _selectedMark;
        }
        set => _selectedMark = value;
    }

    public ObservableCollection<MarkCompleteEventViewModel> ExecutionHistory { get; } = [];

    public ObservableCollection<FillExecutionViewModel> FillExecutionViewModels { get; } = [];

    public ObservableCollection<ExecutorInfo> AvailableExecutors { get; } = [];

    public MarkFillExecutionWindowViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
        ControlTitle = "Выполнение марки";
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        var vms = FillExecutionViewModels.Where(e => e.IsNotFinished).ToList();
        if (vms.Count == 0)
        {
            MessageBoxHelper.ShowInfoBox("Марка уже выполнена на каждом из доступных участков.");
            return;
        }

        var counts = vms.ToDictionary(e => e.Area.Id, e => e.Count?.ParseInDifferentCultures());
        if (counts.Any(e => e.Value is null) || vms.Any(e => e.Date is null))
        {
            MessageBoxHelper.ShowErrorBox("Заполнение даты и количества выполнения обязательно.");
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();
        var dto = new FillExecutionDTO(
            SelectedMark.Id,
            vms.Select(e => new AreaExecutionDTO(e.Area.Id, e.Executors.Select(e => e.Id).ToArray(), (double)counts[e.Area.Id]!, e.Date!.Value.ToUniversalTime(), e.Remark)));

        var result = await markService.FillExecutionAsync(dto);
        if (result.IsSuccess)
        {
            Messenger.Send(new MarkExecutionFilledMessage(dto.MarkId, dto.AreasExecutions.ToDictionary(e => e.AreaId, e => e.Count)));
            _dialogService.CloseDialog();
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }

    }

    protected override bool CanAccept(object p)
    {
        return true;
    }

    protected override async void OnActivated()
    {
        base.OnActivated();

        IsEnabled = false;

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var enabledAreas = await dbContext
            .Projects
            .AsNoTracking()
            .Where(e => e.Id == SelectedMark.ProjectId)
            .SelectMany(e => e.Areas)
            .Select(e => e.Area)
            .OrderBy(e => e.Order)
            .ToListAsync();

        var completeEvents = await dbContext
            .MarkCompleteEvents
            .Include(e => e.Creator)
            .ThenInclude(e => e.Employee)
            .Include(e => e.Executors)
            .ThenInclude(e => e.Employee)
            .Include(e => e.Area)
            .AsNoTracking()
            .Where(e => e.MarkId == SelectedMark.Id)
            .ToListAsync();

        var executors = await dbContext
            .Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => new ExecutorInfo(e.Id, string.IsNullOrWhiteSpace(e.Post) ? e.FullName : $"{e.FullName} ({e.Post})"))
            .ToListAsync();

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            var fillExecutionVms = enabledAreas
            .ToDictionary(e => e, i => completeEvents.Where(e => e.AreaId == i.Id).Sum(e => e.CompleteCount))
            .Select(e => new FillExecutionViewModel { Area = e.Key.ToViewModel(), Left = SelectedMark.Count - e.Value })
            .ToList();

            FillExecutionViewModels.AddRange(fillExecutionVms);
            AvailableExecutors.AddRange(executors);
            ExecutionHistory.AddRange(completeEvents.Select(e => e.ToViewModel()));
            IsEnabled = true;
        });
    }
}

public partial class FillExecutionViewModel : ObservableObject
{
    public required double Left { get; init; }

    public required AreaViewModel Area { get; init; }

    public ObservableCollection<ExecutorInfo> Executors { get; } = [];

    public bool IsNotFinished => Left != 0;

    public bool IsFinished => !IsNotFinished;

    [ObservableProperty]
    public DateTime? _date;

    [ObservableProperty]
    private string? _count;

    [ObservableProperty]
    private string? _remark;

    private ExecutorInfo? _selectedExecutor;

    public ExecutorInfo? SelectedExecutor
    {
        get => _selectedExecutor;
        set
        {
            if (SetProperty(ref _selectedExecutor, value))
            {
                if (value is not null && !Executors.Contains(value))
                {
                    Executors.Add(value);
                }
            }
        }
    }

    [RelayCommand]
    public void RemoveExecutor(object param)
    {
        if (param is not ExecutorInfo executor || !Executors.Contains(executor))
        {
            return;
        }

        Executors.Remove(executor);
        SelectedExecutor = null;
    }
}

public record ExecutorInfo(Guid Id, string FullNameAndPost);

public class MarkCompleteEventViewModel
{
    public required Guid Id { get; init; }

    public required Guid MarkId { get; init; }

    public required DateTimeOffset CreatedDate { get; init; }

    public required double Count { get; init; }

    public required string AreaTitle { get; init; }

    public required string CreatorUserNameAndEmployeeName { get; init; }

    public required IReadOnlyCollection<ExecutorInfo> Executors { get; init; }

    public string? Remark { get; init; }
}