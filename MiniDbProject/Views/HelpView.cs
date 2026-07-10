using MiniDbProject.Constants;
using MiniDbProject.Helpers;
using MiniDbProject.Resources;

namespace MiniDbProject.Views;

public class HelpView : IView
{
    public string Name => "Help";

    public async Task ShowAsync()
    {
        bool stay = true;
        while (stay)
        {
            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader(Messages.Help.Title);

            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuTitle("Getting Started");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}1.{AppConstants.Colors.Reset} {AppConstants.Colors.White}Login:{AppConstants.Colors.Reset} Use demo accounts (admin/Admin@123, user/User@123, demo/Demo@123)");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}2.{AppConstants.Colors.Reset} {AppConstants.Colors.White}Dashboard:{AppConstants.Colors.Reset} Navigate using number keys or arrow keys");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}3.{AppConstants.Colors.Reset} {AppConstants.Colors.White}Create Database:{AppConstants.Colors.Reset} Follow the wizard step by step");
            ConsoleHelper.WriteLineColored("");

            ConsoleRenderer.DrawMenuTitle("Features");
            var features = new (string, string)[]
            {
                ("Profile Management", "View and update your personal information, change password, manage MFA settings."),
                ("Database Wizard", "Guided step-by-step process to create a database from SQL script on your local SQL Server."),
                ("Log Viewer", "Browse, filter, search, and export application logs for monitoring and debugging."),
                ("Help & Support", "This section - documentation and frequently asked questions."),
                ("About", "Application information including version and build details.")
            };
            foreach (var (name, desc) in features)
            {
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}\u25B6{AppConstants.Colors.Reset} {AppConstants.Colors.Bold}{name}{AppConstants.Colors.Reset}");
                ConsoleHelper.WriteLineColored($"    {AppConstants.Colors.Dim}{desc}{AppConstants.Colors.Reset}");
                ConsoleHelper.WriteLineColored("");
            }

            ConsoleRenderer.DrawMenuTitle("Keyboard Shortcuts");
            var shortcuts = new (string, string)[]
            {
                ("\u2191 \u2193", "Navigate menus"),
                ("Enter", "Select menu item"),
                ("ESC", "Go back / Exit / Cancel"),
                ("1-9", "Quick-select menu items"),
                ("B", "Go back to previous screen"),
                ("F1", "Help (where available)"),
                ("F5", "Refresh current view"),
                ("Ctrl+C", "Cancel long-running operations")
            };
            foreach (var (key, desc) in shortcuts)
            {
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}[{key}]{AppConstants.Colors.Reset} {desc}");
            }

            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuTitle("Common Troubleshooting");

            var troubleshoots = new (string, string)[]
            {
                ("Login Failed", "Ensure you are using one of the demo accounts. Check Caps Lock is off."),
                ("Database Creation Fails", "Verify SQL Server LocalDB is installed. Check the script path is valid."),
                ("Logs Not Showing", "Logs are stored in memory for the current session and in Logs/ folder as files."),
                ("MFA Not Working", "Use 123456 as the MFA code for testing. In production, use an authenticator app."),
                ("Portals Not Loading", "This is a demo with mock data. Portals should appear automatically.")
            };
            foreach (var (issue, solution) in troubleshoots)
            {
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Warning}\u26A0{AppConstants.Colors.Reset} {AppConstants.Colors.Bold}{issue}:{AppConstants.Colors.Reset}");
                ConsoleHelper.WriteLineColored($"    {AppConstants.Colors.Dim}{solution}{AppConstants.Colors.Reset}");
                ConsoleHelper.WriteLineColored("");
            }

            ConsoleRenderer.DrawFooter("Press B or ESC to go back");
            ConsoleKey key2;
            try { key2 = Console.ReadKey(true).Key; } catch { return; }
            if (key2 == ConsoleKey.B || key2 == ConsoleKey.Escape)
                stay = false;
        }
    }
}
