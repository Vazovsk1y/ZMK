using Microsoft.Extensions.Hosting;
using ZMK.Wpf.Extensions;

namespace ZMK.Wpf;

internal class Program
{
    public static bool IsInDebug { get; private set; }

    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {
        _mutex = new Mutex(true, App.Title, out bool createdNew);
        if (!createdNew)
        {
            MessageBoxHelper.ShowErrorBox("Приложение уже запущено.");
            return;
        }

#if DEBUG
        IsInDebug = true;
#endif

        App app = new();
        app.StartGlobalExceptionsHandling();
        app.InitializeComponent();
        app.MigrateDatabase();
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", IsInDebug ? "Development" : "Production");

        return Host
            .CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((appConfig, _) =>
            {
                appConfig.HostingEnvironment.ApplicationName = App.Title;
                appConfig.HostingEnvironment.ContentRootPath = Environment.CurrentDirectory;
            })
            .ConfigureServices(Registrator.ConfigureServices);
    }
}