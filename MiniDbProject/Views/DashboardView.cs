using MiniDbProject.Authentication;
using MiniDbProject.Constants;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;
using MiniDbProject.Resources;

namespace MiniDbProject.Views;

public class DashboardView : IView
{
    private readonly IAuthenticationService _authService;
    private readonly ILoggerService _logger;
    private readonly DatabaseWizardView _wizardView;
    private readonly ProfileView _profileView;
    private readonly LogViewerView _logViewerView;
    private readonly HelpView _helpView;
    private readonly AboutView _aboutView;

    public string Name => "Dashboard";

    public DashboardView(
        IAuthenticationService authService,
        ILoggerService logger,
        DatabaseWizardView wizardView,
        ProfileView profileView,
        LogViewerView logViewerView,
        HelpView helpView,
        AboutView aboutView)
    {
        _authService = authService;
        _logger = logger;
        _wizardView = wizardView;
        _profileView = profileView;
        _logViewerView = logViewerView;
        _helpView = helpView;
        _aboutView = aboutView;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                ConsoleHelper.WriteError("Session expired. Please login again.");
                return;
            }

            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayBanner(AppConstants.AppName, AppConstants.AppVersion, AppConstants.Author);

            ConsoleRenderer.DrawHeader(
                string.Format(Messages.Dashboard.Welcome, user.FullName),
                string.Format(Messages.Dashboard.LastLogin, user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A"));

            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Bold}{AppConstants.Colors.Header}Main Menu{AppConstants.Colors.Reset}");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}{new string('\u2500', 30)}{AppConstants.Colors.Reset}");
            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuItem("1", Messages.Dashboard.MenuProfile, "Update profile, change password, manage MFA", AppConstants.Icons.User);
            ConsoleRenderer.DrawMenuItem("2", Messages.Dashboard.MenuCreateDb, "Create a new database from SQL script", AppConstants.Icons.Database);
            ConsoleRenderer.DrawMenuItem("3", Messages.Dashboard.MenuViewLogs, "Browse application logs", AppConstants.Icons.Log);
            ConsoleRenderer.DrawMenuItem("4", Messages.Dashboard.MenuHelp, "View help and documentation", AppConstants.Icons.Help);
            ConsoleRenderer.DrawMenuItem("5", Messages.Dashboard.MenuAbout, "About this application", AppConstants.Icons.Info);
            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuItem("L", Messages.Dashboard.MenuLogout, "Logout from the application", AppConstants.Icons.Exit);
            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawFooter("Select an option or press ESC to exit");

            ConsoleKey key;
            try { key = Console.ReadKey(true).Key; } catch { return; }

            switch (key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    await _profileView.ShowAsync();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    await _wizardView.ShowAsync();
                    break;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    await _logViewerView.ShowAsync();
                    break;
                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    await _helpView.ShowAsync();
                    break;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    await _aboutView.ShowAsync();
                    break;
                case ConsoleKey.L:
                    await _authService.LogoutAsync(user.Username);
                    ConsoleHelper.DisplayNotification(Messages.Login.LogoutSuccess, "success");
                    ConsoleHelper.WaitForKey();
                    return;
                case ConsoleKey.Escape:
                    if (ConsoleHelper.PromptConfirmation(Messages.Common.ExitConfirm))
                    {
                        await _authService.LogoutAsync(user.Username);
                        Environment.Exit(0);
                    }
                    break;
            }
        }
    }
}
