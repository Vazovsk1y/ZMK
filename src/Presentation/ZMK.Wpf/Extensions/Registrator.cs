using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using ZMK.Application.Implementation.Extensions;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.PostgresDAL.Extensions;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels;
using ZMK.Wpf.ViewModels.Area;
using ZMK.Wpf.ViewModels.Employee;
using ZMK.Wpf.ViewModels.Mark;
using ZMK.Wpf.ViewModels.Project;
using ZMK.Wpf.ViewModels.Shipment;
using ZMK.Wpf.ViewModels.User;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.Extensions;

public static class Registrator
{
    private record DatabaseOptions(string ConnectionString) : IDatabaseOptions;
    internal static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        var databaseOptions = new DatabaseOptions(context.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Строка подключения к базе данных не определена."));

        collection.AddDataAccessLayer(databaseOptions);
        collection.AddApplicationLayer();
        collection.AddWPF();
    }

    private static void AddWPF(this IServiceCollection services)
    {
        services.AddSingleton<IUserDialogService, UserDialogService>();
        services.AddTransient<ICurrentSessionProvider, CurrentSessionProvider>();
        services.AddSingleton<StatusPanelViewModel>();
        services.AddSingleton<UsersPanelViewModel>();
        services.AddSingleton<EmployeesPanelViewModel>();
        services.AddSingleton<ProjectsPanelViewModel>();
        services.AddSingleton<AreasPanelViewModel>();

        services.AddTransient<MarkExecutionFillingViewModel>();
        services.AddTransient<MarkExecutionFillingWindow>();

        services.AddWindowWithViewModelSingleton<MainWindow, MainWindowViewModel>();
        services.AddWindowWithViewModelTransient<LoginWindow, LoginWindowViewModel>();
        services.AddWindowWithViewModelTransient<UserAddWindow, UserAddViewModel>();
        services.AddWindowWithViewModelTransient<EmployeeAddWindow, EmployeeAddViewModel>();
        services.AddWindowWithViewModelTransient<ProjectSettingsUpdateWindow, ProjectSettingsUpdateViewModel>();
        services.AddWindowWithViewModelTransient<ProjectAddWindow, ProjectAddViewModel>();
        services.AddWindowWithViewModelTransient<MarkAddWindow, MarkAddViewModel>();
        services.AddWindowWithViewModelTransient<AreaAddWindow, AreaAddViewModel>();
        services.AddWindowWithViewModelTransient<ProjectReportsWindow, ProjectReportsViewModel>();
        services.AddWindowWithViewModelTransient<ShipmentAddWindow, ShipmentAddViewModel>();
        services.AddWindowWithViewModelTransient<ProjectShipmentsWindow, ProjectShipmentsViewModel>();
        services.AddWindowWithViewModelTransient<ProjectExecutionWindow, ProjectExecutionViewModel>();

        services.AddMemoryCache();
    }

    public static IHostBuilder CreateApplicationAssociatedFolder(this IHostBuilder hostBuilder)
    {
        if (!Directory.Exists(App.AssociatedFolder))
        {
            Directory.CreateDirectory(App.AssociatedFolder);
        }

        return hostBuilder;
    }

    private static IServiceCollection AddWindowWithViewModelTransient<TWindow, TViewModel>(this IServiceCollection services)
            where TViewModel : ObservableObject
            where TWindow : Window
            => services
        .AddTransient<TViewModel>()
        .AddTransient(s =>
        {
            var viewModel = s.GetRequiredService<TViewModel>();

            if (viewModel is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = true;
            }

            var window = Activator.CreateInstance<TWindow>();
            window.DataContext = viewModel;
            return window;
        });

    private static IServiceCollection AddWindowWithViewModelSingleton<TWindow, TViewModel>(this IServiceCollection services)
            where TViewModel : ObservableObject
            where TWindow : Window
            => services
        .AddSingleton<TViewModel>()
        .AddSingleton(s =>
        {
            var viewModel = s.GetRequiredService<TViewModel>();
            var window = Activator.CreateInstance<TWindow>();

            if (viewModel is ObservableRecipient observableRecipient)
            {
                observableRecipient.IsActive = true;
            }

            window.DataContext = viewModel;
            return window;
        });
}