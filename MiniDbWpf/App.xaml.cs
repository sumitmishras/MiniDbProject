using System.IO;
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
        try
        {
            DispatcherUnhandledException += (_, args) =>
            {
                var ex = args.Exception;
                var msg = $"Error: {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack:\n{ex.StackTrace}";
                try { File.WriteAllText(@"C:\Temp_MiniDB\crash.txt", msg); } catch { }
                MessageBox.Show(msg, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                var msg = $"Fatal: {ex?.Message}\n\nInner: {ex?.InnerException?.Message}\n\nStack:\n{ex?.StackTrace}";
                try { File.WriteAllText(@"C:\Temp_MiniDB\crash_fatal.txt", msg); } catch { }
                MessageBox.Show(msg, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            var login = new Views.LoginWindow(
                ServiceProvider.GetRequiredService<IAuthenticationService>(),
                ServiceProvider.GetRequiredService<ILoggerService>());
            login.Show();
        }
        catch (Exception ex)
        {
            var msg = $"Startup error: {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack:\n{ex.StackTrace}";
            try { File.WriteAllText(@"C:\Temp_MiniDB\crash.txt", msg); } catch { }
            MessageBox.Show(msg, "Fatal Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
    }
}
