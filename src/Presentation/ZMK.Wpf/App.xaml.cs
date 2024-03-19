using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
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

    private static readonly IHost Host;

    public static readonly string AssociatedFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Title);

    public static ILogger Logger { get; }

    public static IServiceProvider Services { get; }

    private static CurrentSessionViewModel? _currentSession;
    public static CurrentSessionViewModel? CurrentSession 
    {
        get
        {
            return _currentSession;
        }
        set
        {
            if (_currentSession is not null && value is not null)
            {
                throw new InvalidOperationException("Установить значение текущей сессии можно только один раз.");
            }
            _currentSession = value;
        }
    }

    static App()
    {
        Host = Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();
        Services = Host.Services;
        Logger = Services.GetRequiredService<ILogger<App>>();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Host.Start();

        var dialogService = Services.GetRequiredService<IUserDialogService>();
        Logger.LogInformation("Запуск окна входа в аккаунт.");
        dialogService.ShowDialog<LoginWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        using var scope = App.Services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        Logger.LogInformation("Приложение было остановлено.");
        authService.Logout();
    }

    public void MigrateDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();
        dbContext.Database.Migrate();
    }

    public void StartGlobalExceptionsHandling()
    {
        const string MessageTemplate = "Что-то пошло не так в {exceptionSource}.";

        DispatcherUnhandledException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            authService.Logout();
            Logger.LogError(e.Exception, MessageTemplate, nameof(DispatcherUnhandledException));
            e.Handled = true;

            Current?.Shutdown();
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            Logger.LogError(e.ExceptionObject as Exception, MessageTemplate, $"{nameof(AppDomain.CurrentDomain)}.{nameof(AppDomain.CurrentDomain.UnhandledException)}");
            authService.Logout();

            Current?.Shutdown();
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            Logger.LogError(e.Exception, MessageTemplate, nameof(TaskScheduler.UnobservedTaskException));
            authService.Logout();

            Current?.Shutdown();
        };
    }
}