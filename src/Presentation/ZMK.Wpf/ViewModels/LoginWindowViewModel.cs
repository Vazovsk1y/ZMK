using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

internal partial class LoginWindowViewModel : DialogViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _userName;

    [ObservableProperty]
    private string _password = string.Empty;

    public ObservableCollection<string> Logins { get; } = [];

    public LoginWindowViewModel()
    {
        ControlTitle = "Login";
    }

    public LoginWindowViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
        ControlTitle = "Login";
    }

    protected override bool CanAccept(object p) => !string.IsNullOrWhiteSpace(UserName);

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var result = await authService.LoginAsync(new UserLoginDTO(UserName, Password)).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            Guid sessionId = result.Value;
            var session = await dbContext
                .Sessions
                .AsNoTracking()
                .Include(e => e.User)
                .ThenInclude(e => e!.Roles)
                .ThenInclude(e => e.Role)
                .Include(e => e.User!.Employee)
                .SingleAsync(e => e.Id == sessionId);


            App.CurrentSession = session.ToViewModel();
            App.Current.Dispatcher.Invoke(() => 
            {
                App.Services.GetRequiredService<MainWindow>().Show();
                _dialogService.CloseDialog();
            });
        }
        else
        {
            MessageBoxHelper.ShowInfoBox(result.Errors.Display());
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();
        var logins = dbContext.Users.Select(e => e.UserName).ToList();
        Logins!.AddRange(logins);
    }
}