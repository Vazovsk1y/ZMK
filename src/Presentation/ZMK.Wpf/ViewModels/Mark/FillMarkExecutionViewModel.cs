using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZMK.Wpf.ViewModels;

public partial class FillMarkExecutionViewModel : ObservableObject
{
    public required double LeftCount { get; init; }

    public required AreaViewModel Area { get; init; }

    public required bool AreExecutorsRequired { get; init; }

    public FillMarkExecutionViewModel? Next { get; set; }

    public bool IsFirst { get; init; }

    public ObservableCollection<ExecutorInfo> Executors { get; } = [];

    public bool IsNotFinished => LeftCount != 0;

    public bool IsFinished => !IsNotFinished;

    [ObservableProperty]
    public DateTime? _date;

    [ObservableProperty]
    private string? _count;

    [ObservableProperty]
    private string? _remark;

    private bool _isAbleToFill;
    public bool IsAbleToFill
    {
        get => _isAbleToFill;
        set
        {
            SetProperty(ref _isAbleToFill, value);
            if (Next is null)
            {
                return;
            }

            if (IsFinished)
            {
                Next.IsAbleToFill = IsAbleToFill;
            }
            else if (value && Executors.Count > 0)
            {
                Next.IsAbleToFill = true;
            }
            else if (!value)
            {
                Next.IsAbleToFill = false;
            }
        }
    }

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
                    if (AreExecutorsRequired && Next is not null)
                    {
                        Next.IsAbleToFill = true;
                    }
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
        if (AreExecutorsRequired && Next is not null && Executors.Count == 0)
        {
            Next.IsAbleToFill = false;
        }
        SelectedExecutor = null;
    }
}