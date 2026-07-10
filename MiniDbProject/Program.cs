using Microsoft.Extensions.DependencyInjection;
using MiniDbProject.Authentication;
using MiniDbProject.Configuration;
using MiniDbProject.Constants;
using MiniDbProject.Database;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Services;
using MiniDbProject.Views;

namespace MiniDbProject;

public class Program
{
    public static async Task Main(string[] args)
    {
        try { Console.Title = AppConstants.AppName; } catch { }

        try
        {
            var config = AppConfiguration.Load();
            ConsoleHelper.SetConsoleDefaults(
                config.ApplicationSettings.ConsoleWidth,
                config.ApplicationSettings.ConsoleHeight);
            ConsoleHelper.DetectConsole();

            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var app = serviceProvider.GetRequiredService<Application>();
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayNotification($"Fatal Error: {ex.Message}", "error");
            ConsoleHelper.WaitForKey("Press any key to exit...");
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IProfileService, ProfileService>();
        services.AddSingleton<IDatabaseService, DatabaseService>();
        services.AddSingleton<IProgressTracker, ProgressTracker>();

        services.AddTransient<LoginView>();
        services.AddTransient<ProfileView>();
        services.AddTransient<LogViewerView>();
        services.AddTransient<HelpView>();
        services.AddTransient<AboutView>();
        services.AddTransient<ProgressView>();
        services.AddTransient<DatabaseWizardView>();
        services.AddTransient<DashboardView>();

        services.AddTransient<Application>();
    }
}

public class Application
{
    private readonly LoginView _loginView;
    private readonly DashboardView _dashboardView;
    private readonly ILoggerService _logger;

    public Application(LoginView loginView, DashboardView dashboardView, ILoggerService logger)
    {
        _loginView = loginView;
        _dashboardView = dashboardView;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        try
        {
            await _logger.LogInfo("Application started.");

            while (true)
            {
                await _loginView.ShowAsync();
                await _dashboardView.ShowAsync();

                ConsoleHelper.ClearScreen();
                if (!ConsoleHelper.PromptConfirmation("Do you want to login again?"))
                    break;
            }

            await _logger.LogInfo("Application closed.");
        }
        catch (Exception ex)
        {
            await _logger.LogError("Unhandled application exception", ex);
            ConsoleHelper.DisplayNotification($"Unexpected error: {ex.Message}", "error");
            ConsoleHelper.WaitForKey();
        }
    }
}
