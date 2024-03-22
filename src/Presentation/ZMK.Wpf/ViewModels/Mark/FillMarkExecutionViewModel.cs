using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ZMK.Wpf.ViewModels;

public partial class FillMarkExecutionViewModel : ObservableObject
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