using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.ViewModels.Area;
using ZMK.Wpf.ViewModels.Project;
using ZMK.Domain.Entities;

namespace ZMK.Wpf.ViewModels.Mark;

public partial class MarkExecutionByAreaViewModel : ObservableObject
{
    public required double LeftCount { get; init; }

    public required AreaViewModel Area { get; init; }

    public required bool AreExecutorsRequired { get; init; }

    public MarkExecutionByAreaViewModel? Next { get; set; }

    public MarkExecutionByAreaViewModel? Previous { get; set; }

    public bool IsFirst { get; init; }

    public ObservableCollection<ExecutorInfo> Executors { get; } = [];

    public string? ExecutorFilterText
    {
        get => throw new NotImplementedException();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AvailableExecutors.Filter = null;
                IsDropDownOpen = false;
                return;
            }

            AvailableExecutors.Filter = item => ((ExecutorInfo)item).FullNameAndPost.Contains(value, StringComparison.OrdinalIgnoreCase);
            IsDropDownOpen = true;
        }
    }

    public required ICollectionView AvailableExecutors { get; init; }

    public bool IsNotFinished => LeftCount != 0;

    public bool IsFinished => !IsNotFinished;

    [ObservableProperty]
    public DateTime? _date = DateTime.Today;

    [ObservableProperty]
    private bool _isDropDownOpen;

    [ObservableProperty]
    private string? _remark;

    private bool _isEmpty;
    public bool IsEmpty
    {
        get => _isEmpty;
        set
        {
            SetProperty(ref _isEmpty, value);
            if (Next is null)
            {
                return;
            }

            Next.IsAbleToFill = CalculateForNext();
        }
    }

    private string? _count;
    public string? Count
    {
        get => _count;
        set
        {
            if (SetProperty(ref _count, value))
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (value == "0" || value == "0," || value == "0.")
                    {
                        return;
                    }

                    var isValid = value.ParseInDifferentCultures() is double count && Domain.Entities.Mark.IsValidCount(count);
                    if (!isValid)
                    {
                        MessageBoxHelper.ShowErrorBox($"Количество выполненных марок должно быть целое число больше нуля или кратное {Domain.Entities.Mark.CountMultiplicityNumber}.");
                        _count = null;
                    }
                }
            }
            ExecutorFilterText = null;
        }
    }

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

            Next.IsAbleToFill = CalculateForNext();
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

    private bool CalculateForNext()
    {
        if (IsFinished)
        {
            return true;
        }

        if (IsEmpty)
        {
            if (IsFirst || Previous is null)
            {
                return true;
            }

            MarkExecutionByAreaViewModel? currentElement = this;
            while (!currentElement?.Previous?.IsEmpty is false && currentElement?.Previous?.IsFinished is false)
            {
                currentElement = currentElement?.Previous;
            }

            return currentElement is null ? Executors.Count > 0 : currentElement.IsAbleToFill;
        }

        if (IsAbleToFill)
        {
            return Executors.Count > 0;
        }
        else
        {
            return false;
        }
    }
}