using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

public partial class EmployeesPanelViewModel : ObservableRecipient, 
    IRecipient<EmployeeAddedMessage>,
    IRefrashable
{
    public ObservableCollection<EmployeeViewModel> Employees { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private EmployeeViewModel? _selectedEmployee;

    public EmployeesPanelViewModel()
    {
        Employees.CollectionChanged += (_, _) =>
        {
            DeleteCommand.NotifyCanExecuteChanged();
        };
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<EmployeeAddWindow>();
    }

    [RelayCommand]
    public void RollbackChanges()
    {
        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var modifiedEmployees = Employees.Where(e => e.IsModified()).ToList();
            if (modifiedEmployees.Count == 0)
            {
                return;
            }

            modifiedEmployees.ForEach(e => e.RollBackChanges());
        }
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo(
            $"Вы уверены, что желаете удалить данного сотрудника '{SelectedEmployee!.FullName}'?" +
            $"\nВнимание! При удалении сотрудника удалятся все связанные с ним пользователи.");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await employeeService.DeleteAsync(SelectedEmployee.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Employees.Remove(SelectedEmployee));
                MessageBoxHelper.ShowInfoBox("Сотрудник успешно удален.");
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() => SelectedEmployee is not null && Employees.Count > 1 && SelectedEmployee?.Id != App.CurrentSession?.User.Employee.Id;

    [RelayCommand]
    public async Task SaveChanges()
    {
        var modifiedEmployees = Employees.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedEmployees.Count == 0)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();

        bool currentEmployeeUpdated = false;
        var results = new List<Result>();
        foreach (var employee in modifiedEmployees)
        {
            var dto = new EmployeeUpdateDTO(employee.Id, employee.FullName, employee.Post, employee.Remark);

            var updateResult = await employeeService.UpdateAsync(dto);
            if (updateResult.IsFailure)
            {
                employee.RollBackChanges();
            }
            else
            {
                if (employee.Id == App.CurrentSession?.User.Employee.Id)
                {
                    currentEmployeeUpdated = true;
                }
                employee.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Where(e => e.IsSuccess).Any())
        {
            MessageBoxHelper.ShowInfoBox($"Информация об {results.Where(e => e.IsSuccess).Count()} сотрудниках была обновлена успешно.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(results.Where(e => e.IsFailure).SelectMany(e => e.Errors).Display());
        }

        if (currentEmployeeUpdated)
        {
            MessageBoxHelper.ShowInfoBox("Необходимо перезайти используя обновленные данные.");
            App.Current.Shutdown();
        }
    }

    public void Receive(EmployeeAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Employees.Add(message.Employee);
            MessageBoxHelper.ShowInfoBox("Сотрудник был успешно добавлен.");
        });
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var employees = await dbContext
            .Employees
            .AsNoTracking()
            .Select(e => e.ToPanelViewModel())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Employees.Clear();
            SelectedEmployee = null;
            Employees.AddRange(employees);
        });
    }
}