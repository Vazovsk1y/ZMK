using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf;

public partial class App : System.Windows.Application
{
    public const string Title = "ZMK";

    private static readonly IHost Host = Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

    public static IServiceProvider Services => Host.Services;

    public static CurrentSessionViewModel? CurrentSession { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Host.Start();
        var dialogService = Services.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<LoginWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        using var scope = App.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        authService.Logout();
    }

    public void MigrateDatabase()
    {
        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();
        dbContext.Database.Migrate();
    }
}