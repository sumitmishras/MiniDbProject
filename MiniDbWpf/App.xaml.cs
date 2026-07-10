using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MiniDbWpf.Services;

namespace MiniDbWpf;

public partial class App : Application
{
    public static ServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show($"Error: {args.Exception.Message}", "Application Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            MessageBox.Show($"Fatal: {ex?.Message}", "Fatal Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var login = new Views.LoginWindow(
            ServiceProvider.GetRequiredService<IAuthenticationService>(),
            ServiceProvider.GetRequiredService<ILoggerService>());
        login.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
    }
}
