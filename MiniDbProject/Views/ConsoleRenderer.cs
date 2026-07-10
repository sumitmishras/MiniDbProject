using MiniDbProject.Constants;
using MiniDbProject.Helpers;

namespace MiniDbProject.Views;

public static class ConsoleRenderer
{
    public static void Initialize(int width = AppConstants.DefaultConsoleWidth, int height = AppConstants.DefaultConsoleHeight)
    {
        ConsoleHelper.SetConsoleDefaults(width, height);
    }

    public static void DrawHeader(string title, string? subtitle = null)
    {
        int w = SafeWidth();
        ConsoleHelper.WriteLineColored($"{AppConstants.Colors.Primary}{new string('\u2550', w)}{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"{AppConstants.Colors.Bold}{AppConstants.Colors.Header}  {title}{AppConstants.Colors.Reset}");
        if (!string.IsNullOrEmpty(subtitle))
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}{subtitle}{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"{AppConstants.Colors.Primary}{new string('\u2550', w)}{AppConstants.Colors.Reset}");
    }

    public static void DrawFooter(string? message = null)
    {
        int w = SafeWidth();
        ConsoleHelper.WriteLineColored($"{AppConstants.Colors.Primary}{new string('\u2550', w)}{AppConstants.Colors.Reset}");
        if (!string.IsNullOrEmpty(message))
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}{message}{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}F1=Help | ESC=Exit | B=Back{AppConstants.Colors.Reset}");
    }

    public static void DrawMenuTitle(string title)
    {
        ConsoleHelper.WriteLineColored("");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Bold}{AppConstants.Colors.Header}{title}{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}{new string('\u2500', title.Length + 2)}{AppConstants.Colors.Reset}");
    }

    public static void DrawMenuItem(string key, string label, string? description = null, string icon = "")
    {
        string iconStr = string.IsNullOrEmpty(icon) ? " " : icon;
        ConsoleHelper.WriteColored($"    {AppConstants.Colors.Primary}[{key}]{AppConstants.Colors.Reset} ");
        ConsoleHelper.WriteColored($"{AppConstants.Colors.White}{label}{AppConstants.Colors.Reset}");
        if (!string.IsNullOrEmpty(description))
            ConsoleHelper.WriteColored($"  {AppConstants.Colors.Dim}- {description}{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored("");
    }

    public static void DrawInfoPanel(string title, Dictionary<string, string> items)
    {
        int w = SafeWidth() - 6;
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}\u2554{new string('\u2550', w)}\u2557{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}\u2551{AppConstants.Colors.Reset} {AppConstants.Colors.Bold}{AppConstants.Colors.Header}{title.PadRight(w - 2)}{AppConstants.Colors.Reset} {AppConstants.Colors.Primary}\u2551{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}\u2560{new string('\u2550', w)}\u2563{AppConstants.Colors.Reset}");
        foreach (var item in items)
        {
            string line = $"  {AppConstants.Colors.Primary}\u2551{AppConstants.Colors.Reset} {AppConstants.Colors.Dim}{item.Key}:{AppConstants.Colors.Reset} {item.Value}".PadRight(w + 10);
            ConsoleHelper.WriteLineColored($"{line} {AppConstants.Colors.Primary}\u2551{AppConstants.Colors.Reset}");
        }
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Primary}\u255A{new string('\u2550', w)}\u255D{AppConstants.Colors.Reset}");
    }

    public static void DrawBox(string title, string content, ConsoleColor borderColor = ConsoleColor.Cyan)
    {
        ConsoleHelper.DisplayPanel(title, content);
    }

    public static void DrawProgressBar(int percentage, int width = AppConstants.ProgressBarWidth)
    {
        ConsoleHelper.WriteProgressBar(percentage, width);
    }

    private static int SafeWidth()
    {
        try { return Console.WindowWidth; } catch { return 80; }
    }
}
