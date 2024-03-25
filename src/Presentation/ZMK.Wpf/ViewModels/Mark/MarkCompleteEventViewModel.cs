using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZMK.Wpf.ViewModels;

public partial class MarkCompleteEventViewModel :
    MarkEventViewModel,
    IModifiable<MarkCompleteEventViewModel>
{
    public MarkCompleteEventViewModel PreviousState { get; private set; } = default!;

    public UpdatableSign? UpdatableSign => IsModified() ? new UpdatableSign() : null;

    private AreaInfo _area = null!;
    public AreaInfo Area
    {
        get => _area;
        set 
        {
            SetProperty(ref _area, value);
            OnPropertyChanged(nameof(UpdatableSign));
            CommonTitle = value.Title;
        }
    }

    [ObservableProperty]
    private ObservableCollection<ExecutorInfo> _executors = [];

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
                    OnPropertyChanged(nameof(UpdatableSign));
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
        OnPropertyChanged(nameof(UpdatableSign));
        SelectedExecutor = null;
    }

    private DateTime _date;
    public override DateTime Date
    {
        get => _date; 
        set
        {
            if (SetProperty(ref _date, value))
            {
                OnPropertyChanged(nameof(UpdatableSign));
            }
        }
    }

    private double _markCount;
    public override double MarkCount                                                            
    {
        get => _markCount;
        set
        {
            if (SetProperty(ref _markCount, value))
            {
                OnPropertyChanged(nameof(UpdatableSign));
            }
        }
    }

    private string? _remark;
    public override string? Remark
    {
        get => _remark;
        set
        {
            if (SetProperty(ref _remark, value))
            {
                OnPropertyChanged(nameof(UpdatableSign));
            }
        }
    }

    public bool IsModified()
    {
        return Area.Id != PreviousState.Area.Id
            || Date != PreviousState.Date
            || MarkCount != PreviousState.MarkCount
            || Remark != PreviousState.Remark
            || !Executors.OrderBy(e => e.FullNameAndPost).Select(e => e.Id).SequenceEqual(PreviousState.Executors.OrderBy(e => e.FullNameAndPost).Select(e => e.Id));
    }

    public void RollBackChanges()
    {
        Area = PreviousState.Area;
        Date = PreviousState.Date;
        MarkCount = PreviousState.MarkCount;
        Remark = PreviousState.Remark;
        Executors = new (PreviousState.Executors);
        SelectedExecutor = null;
        OnPropertyChanged(nameof(UpdatableSign));
    }

    public virtual void SaveState()
    {
        PreviousState = (MarkCompleteEventViewModel)MemberwiseClone();
        PreviousState.Executors = new(Executors);
        OnPropertyChanged(nameof(UpdatableSign));
    }
}
