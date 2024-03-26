using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Constants;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class UsersPanelViewModel : TitledViewModel,
    IRecipient<UserAddedMessage>,
    IRefrashable
{
    public ObservableCollection<UserViewModel.RoleInfo> AvailableRoles { get; } = [];
    public ObservableCollection<UserViewModel.EmployeeInfo> AvailableEmployees { get; } = [];
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
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() 
    {
        if (SelectedUser is UserViewModel u && u.Role.Name == DefaultRoles.Admin)
        {
            if (Users.Select(e => e.Role).Where(e => e.Name == DefaultRoles.Admin).Count() == 1)
            {
                return false;
            }
        }

        return SelectedUser is not null && Users.Count > 1 && SelectedUser?.Id != App.CurrentSession?.User.Id;
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<UserAddWindow>();
    }

    [RelayCommand]
    public void RollbackChanges()
    {
        var modifiedUsers = Users.Where(e => e.IsModified()).ToList();
        if (modifiedUsers.Count == 0)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            modifiedUsers.ForEach(e => e.RollBackChanges());
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        var modifiedUsers = Users.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedUsers.Count == 0)
        {
            return;
        }

        IsEnabled = false;
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
                currentUserUpdated = user.Id == App.CurrentSession?.User.Id;
                user.SaveState();
            }

            results.Add(updateResult);
        }

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;

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
        });
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IsEnabled = false;

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
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);


        var roles = await dbContext
            .Roles
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => e.ToPanelViewModel())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var employees = await dbContext
            .Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => e.ToUsersPanelViewModel())
            .ToListAsync(cancellationToken);


        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Users.Clear();
            AvailableRoles.Clear();
            AvailableEmployees.Clear();
            SelectedUser = null;

            Users.AddRange(users);
            AvailableRoles.AddRange(roles);
            AvailableEmployees.AddRange(employees);
            IsEnabled = true;
        });
    }
}
