using CommunityToolkit.Mvvm.ComponentModel;
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
using static ZMK.Wpf.ViewModels.UserViewModel;

namespace ZMK.Wpf.ViewModels;

public partial class UserAddViewModel : DialogViewModel
{
    public ObservableCollection<RoleViewModel> AvailableRoles { get; } = [];
    public ObservableCollection<EmployeeViewModel> AvailableEmployees { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _userName = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private RoleViewModel? _selectedRole;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private EmployeeViewModel? _selectedEmployee;

    [ObservableProperty]
    private string _password = null!;

    public UserAddViewModel()
    {
        ControlTitle = "Добавление пользователя";
    }

    public UserAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var result = await userService.AddAsync(new UserAddDTO(UserName, Password, SelectedRole!.Id, SelectedEmployee!.Id));
        if (result.IsSuccess)
        {
            _dialogService.CloseDialog();
            Messenger.Send(new UserAddedMessage(this.ToPanelViewModel(result.Value)));
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p) => !string.IsNullOrWhiteSpace(UserName)
        && SelectedRole is not null
        && SelectedEmployee is not null;

    protected override async void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

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
            AvailableRoles.AddRange(roles);
            AvailableEmployees.AddRange(employees);
        });
    }
}
