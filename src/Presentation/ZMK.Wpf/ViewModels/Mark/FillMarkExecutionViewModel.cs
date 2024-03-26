using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZMK.Domain.Entities;
using ZMK.Wpf.Extensions;

namespace ZMK.Wpf.ViewModels;

public partial class FillMarkExecutionViewModel : ObservableObject
{
    public required double LeftCount { get; init; }

    public required AreaViewModel Area { get; init; }

    public required bool AreExecutorsRequired { get; init; }

    public FillMarkExecutionViewModel? Next { get; set; }

    public FillMarkExecutionViewModel? Previous { get; set; }

    public bool IsFirst { get; init; }

    public ObservableCollection<ExecutorInfo> Executors { get; } = [];

    public bool IsNotFinished => LeftCount != 0;

    public bool IsFinished => !IsNotFinished;

    [ObservableProperty]
    public DateTime? _date = DateTime.Today;

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

                    var isValid = value.ParseInDifferentCultures() is double count && Mark.IsValidCount(count);
                    if (!isValid)
                    {
                        MessageBoxHelper.ShowErrorBox($"Количество выполненных марок должно быть целое число больше нуля или кратное {Mark.CountMultiplicityNumber}.");
                        _count = null;
                    }
                }
            }
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

            FillMarkExecutionViewModel? currentElement = this;
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