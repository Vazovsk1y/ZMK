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

    public ObservableCollection<FillMarkExecutionViewModel> FillExecutionViewModels { get; } = [];

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
        var dto = new FillMarkExecutionDTO(
            SelectedMark.Id,
            vms.Select(e => new AreaExecutionDTO(e.Area.Id, e.Executors.Select(e => e.Id).ToArray(), (double)counts[e.Area.Id]!, e.Date!.Value.ToUniversalTime(), e.Remark)));

        var result = await markService.FillExecutionAsync(dto);
        if (result.IsSuccess)
        {
            Messenger.Send(new MarkExecutionFilledMessage(dto.MarkId, dto.Executions.ToDictionary(e => e.AreaId, e => e.Count)));
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
            .Include(e => e.Settings)
            .AsNoTracking()
            .Where(e => e.Id == SelectedMark.ProjectId)
            .SelectMany(e => e.Areas)
            .Select(e => new { e.Area, e.Project.Settings.AreExecutorsRequired })
            .OrderBy(e => e.Area.Order)
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
            .Select(e => e.ToInfo())
            .ToListAsync();

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            var execution = enabledAreas.ToDictionary(e => e, i => completeEvents.Where(e => e.AreaId == i.Area.Id).Sum(e => e.CompleteCount));
            var fillExecutionVms = new ObservableCollection<FillMarkExecutionViewModel>();

            var enumerator = execution.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var previous = new FillMarkExecutionViewModel
                {
                    Area = current.Key.Area.ToViewModel(),
                    LeftCount = SelectedMark.Count - current.Value,
                    AreExecutorsRequired = current.Key.AreExecutorsRequired,
                    IsAbleToFill = true,
                    IsFirst = true,
                };

                FillExecutionViewModels.Add(previous);

                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    var vm = new FillMarkExecutionViewModel
                    {
                        Area = current.Key.Area.ToViewModel(),
                        LeftCount = SelectedMark.Count - current.Value,
                        AreExecutorsRequired = current.Key.AreExecutorsRequired,
                        IsAbleToFill = previous.IsFinished && previous.IsFirst || !current.Key.AreExecutorsRequired,
                        IsFirst = previous.IsFinished && previous.IsFirst,
                    };

                    FillExecutionViewModels.Add(vm);
                    previous.Next = vm;
                    previous = vm;
                }
            }

            AvailableExecutors.AddRange(executors);
            ExecutionHistory.AddRange(completeEvents.Select(e => e.ToViewModel()));
            IsEnabled = true;
        });
    }
}