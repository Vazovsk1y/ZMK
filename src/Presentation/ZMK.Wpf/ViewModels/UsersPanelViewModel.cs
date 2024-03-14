using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows.Media;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

internal partial class UsersPanelViewModel : ObservableRecipient,
    IRecipient<UserAddedMessage>
{
    public ObservableCollection<UserViewModel.RoleViewModel> AvailableRoles { get; } = [];
    public ObservableCollection<UserViewModel.EmployeeViewModel> AvailableEmployees { get; } = [];
    public ObservableCollection<UserViewModel> Users { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private UserViewModel? _selectedUser;

    public UsersPanelViewModel()
    {
        Users.CollectionChanged += (_, _) =>
        {
            DeleteCommand.NotifyCanExecuteChanged();
        };
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете удалить пользователя '{SelectedUser!.UserName}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await userService.DeleteAsync(SelectedUser.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Users.Remove(SelectedUser));
                MessageBoxHelper.ShowInfoBox("Пользователь успешно удален.");
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() => SelectedUser is not null && Users.Count > 1;

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<UserAddWindow>();
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        var modifiedUsers = Users.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedUsers.Count == 0)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        bool currentUserUpdated = false;
        var results = new List<Result>();
        foreach (var user in modifiedUsers)
        {
            var dto = new UserUpdateDTO(user.Id, user.UserName, user.Password, user.Role.Id, user.Employee.Id);

            var updateResult = await userService.UpdateAsync(dto);
            if (updateResult.IsFailure)
            {
                user.RollBackChanges();
            }
            else
            {
                if (user.Id == App.CurrentSession?.User.Id)
                {
                    currentUserUpdated = true;
                }
                user.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Where(e => e.IsSuccess).Any())
        {
            MessageBoxHelper.ShowInfoBox($"Информация об {results.Where(e => e.IsSuccess).Count()} пользователях была обновлена успешно.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(results.Where(e => e.IsFailure).SelectMany(e => e.Errors).Display());
        }

        if (currentUserUpdated)
        {
            MessageBoxHelper.ShowInfoBox("Необходимо перезайти используя обновленные данные.");
            App.Current.Shutdown();
        }
    }

    public void Receive(UserAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Users.Add(message.User);
            MessageBoxHelper.ShowInfoBox("Пользователь был успешно добавлен.");
        });
    }

    protected override async void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var users = await dbContext
            .Users
            .AsNoTracking()
            .Include(e => e.Roles)
            .ThenInclude(e => e.Role)
            .Include(e => e.Employee)
            .OrderBy(e => e.UserName)
            .Select(e => e.ToPanelViewModel())
            .ToListAsync();

        var roles = await dbContext
            .Roles
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => e.ToPanelViewModel())
            .ToListAsync();

        var employees = await dbContext
            .Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => e.ToPanelViewModel())
            .ToListAsync();


        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Users.AddRange(users);
            AvailableRoles.AddRange(roles);
            AvailableEmployees.AddRange(employees);
        });
    }
}
