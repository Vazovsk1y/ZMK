﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

    public void StartGlobalExceptionsHandling()
    {
        const string MessageTemplate = "Что-то пошло не так в {exceptionSource}.";

        DispatcherUnhandledException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            logger.LogError(e.Exception, MessageTemplate, nameof(DispatcherUnhandledException));
            e.Handled = true;

            Current?.Shutdown();
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            logger.LogError(e.ExceptionObject as Exception, MessageTemplate, $"{nameof(AppDomain.CurrentDomain)}.{nameof(AppDomain.CurrentDomain.UnhandledException)}");
            authService.Logout();

            Current?.Shutdown();
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            using var scope = Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<App>>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            logger.LogError(e.Exception, MessageTemplate, nameof(TaskScheduler.UnobservedTaskException));
            authService.Logout();

            Current?.Shutdown();
        };
    }
}