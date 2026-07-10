using MiniDbProject.Constants;
using MiniDbProject.Helpers;

namespace MiniDbProject.Views;

public class AboutView : IView
{
    public string Name => "About";

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DisplayBanner(AppConstants.AppName, AppConstants.AppVersion, AppConstants.Author);

        ConsoleRenderer.DrawHeader("About This Application");

        ConsoleHelper.WriteLineColored("");

        var info = new Dictionary<string, string>
        {
            { "Application Name", AppConstants.AppName },
            { "Version", AppConstants.AppVersion },
            { "Author", AppConstants.Author },
            { "Build Date", "2026-07-10" },
            { "Framework", AppConstants.FrameworkVersion },
            { "Platform", "Windows" },
            { "Language", "C# 13" },
            { "License", "MIT License" },
            { "Repository", "https://github.com/yourusername/minidbproject" }
        };
        ConsoleRenderer.DrawInfoPanel("Application Information", info);

        ConsoleHelper.WriteLineColored("");
        var tech = new Dictionary<string, string>
        {
            { ".NET Version", "NET 10.0" },
            { "Dependency Injection", "Microsoft.Extensions.DependencyInjection" },
            { "Configuration", "Microsoft.Extensions.Configuration.Json" },
            { "Logging", "Custom In-Memory + File Logger" },
            { "Authentication", "Custom Authentication Service (Stub)" },
            { "Password Hashing", "PBKDF2 with SHA-256 (RFC 2898)" },
            { "Console UI", "ANSI Escape Codes + Unicode Characters" }
        };
        ConsoleRenderer.DrawInfoPanel("Technology Stack", tech);

        ConsoleHelper.WriteLineColored("");
        ConsoleRenderer.DrawMenuTitle("Description");
        string description = "Mini Database Project is a console-based application that allows users to securely " +
            "log in, manage their profile, and create local databases from SQL scripts hosted on remote " +
            "server portals. It features a modern console UI with real-time progress tracking, " +
            "comprehensive logging, and a clean architecture designed for future extensibility.";
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}{description}{AppConstants.Colors.Reset}");

        ConsoleHelper.WriteLineColored("");
        ConsoleRenderer.DrawFooter("Press any key to go back");
        await Task.CompletedTask;
        try { Console.ReadKey(true); } catch { }
    }
}
