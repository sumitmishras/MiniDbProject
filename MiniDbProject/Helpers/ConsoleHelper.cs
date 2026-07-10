using MiniDbProject.Constants;

namespace MiniDbProject.Helpers;

public static class ConsoleHelper
{
    private static readonly object _lock = new();

    public static void SetConsoleDefaults(int width, int height)
    {
        try
        {
            Console.WindowWidth = Math.Min(width, Console.LargestWindowWidth);
            Console.WindowHeight = Math.Min(height, Console.LargestWindowHeight);
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Math.Max(1000, Console.WindowHeight * 3);
            Console.Title = AppConstants.AppName;
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
        }
        catch { }
    }

    private static void SafeConsoleWrite(string text)
    {
        if (!_hasConsole) return;
        try { Console.Write(text); } catch { _hasConsole = false; }
    }

    private static void SafeConsoleWriteLine(string text)
    {
        if (!_hasConsole) return;
        try { Console.WriteLine(text); } catch { _hasConsole = false; }
    }

    public static void WriteColored(string text, string color = AppConstants.Colors.Default, bool resetLine = false)
    {
        lock (_lock)
        {
            SafeConsoleWrite($"{color}{text}{AppConstants.Colors.Reset}");
            if (resetLine) SafeConsoleWriteLine("");
        }
    }

    public static void WriteLineColored(string text, string color = AppConstants.Colors.Default)
    {
        lock (_lock)
        {
            SafeConsoleWriteLine($"{color}{text}{AppConstants.Colors.Reset}");
        }
    }

    public static void WriteSuccess(string message)
    {
        WriteLineColored($" {AppConstants.Icons.Success} {message}", AppConstants.Colors.Success);
    }

    public static void WriteError(string message)
    {
        WriteLineColored($" {AppConstants.Icons.Error} {message}", AppConstants.Colors.Error);
    }

    public static void WriteWarning(string message)
    {
        WriteLineColored($" {AppConstants.Icons.Warning} {message}", AppConstants.Colors.Warning);
    }

    public static void WriteInfo(string message)
    {
        WriteLineColored($" {AppConstants.Icons.Info} {message}", AppConstants.Colors.Info);
    }

    public static void WriteHeader(string text)
    {
        WriteLineColored($" {AppConstants.Colors.Bold}{text}", AppConstants.Colors.Header);
    }

    public static void WriteDivider(char c = '\u2550', string? color = null)
    {
        color ??= AppConstants.Colors.Dim;
        int width = SafeGetWindowWidth();
        WriteLineColored(new string(c, Math.Max(10, width - 1)), color);
    }

    public static void WriteTableHeader(string[] columns, int[] widths)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(AppConstants.Colors.Header);
        sb.Append(AppConstants.Colors.Bold);
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(' ');
            sb.Append(columns[i].PadRight(widths[i]));
            sb.Append(" ");
        }
        sb.Append(AppConstants.Colors.Reset);
        SafeConsoleWriteLine(sb.ToString());
        WriteDivider('\u2500', AppConstants.Colors.Dim);
    }

    public static void WriteTableRow(string[] values, int[] widths, string color = "")
    {
        if (string.IsNullOrEmpty(color)) color = AppConstants.Colors.Default;
        var sb = new System.Text.StringBuilder();
        sb.Append(color);
        for (int i = 0; i < values.Length; i++)
        {
            sb.Append(' ');
            sb.Append((values[i] ?? "").PadRight(widths[i]));
            sb.Append(' ');
        }
        sb.Append(AppConstants.Colors.Reset);
        SafeConsoleWriteLine(sb.ToString());
    }

    private static int SafeGetWindowWidth()
    {
        if (!_hasConsole) return 80;
        try { return Console.WindowWidth; } catch { _hasConsole = false; return 80; }
    }

    public static bool _hasConsole = true;

    public static void DetectConsole()
    {
        try
        {
            int _ = Console.WindowWidth;
            _hasConsole = true;
        }
        catch
        {
            _hasConsole = false;
        }
    }

    public static void ClearScreen()
    {
        try
        {
            Console.Clear();
        }
        catch
        {
            _hasConsole = false;
        }
    }

    public static void DisplayPanel(string title, string content, string borderColor = "")
    {
        if (!_hasConsole) { SafeConsoleWriteLine(content); return; }
        if (string.IsNullOrEmpty(borderColor)) borderColor = AppConstants.Colors.Primary;
        int width = SafeGetWindowWidth() - 2;
        try
        {
            string top = $"{borderColor}\u2554{new string('\u2550', width)}\u2557{AppConstants.Colors.Reset}";
            string titleLine = $"{borderColor}\u2551{AppConstants.Colors.Bold}{AppConstants.Colors.Header} {title.PadRight(width - 1)}{AppConstants.Colors.Reset}{borderColor}\u2551{AppConstants.Colors.Reset}";
            string sep = $"{borderColor}\u2560{new string('\u2550', width)}\u2563{AppConstants.Colors.Reset}";
            string bottom = $"{borderColor}\u255A{new string('\u2550', width)}\u255D{AppConstants.Colors.Reset}";

            SafeConsoleWriteLine(top);
            SafeConsoleWriteLine(titleLine);
            SafeConsoleWriteLine(sep);

            foreach (var line in content.Split('\n'))
            {
                string trimmed = line.TrimEnd().Length > width ? line.TrimEnd()[..width] : line.TrimEnd();
                SafeConsoleWriteLine($"{borderColor}\u2551{AppConstants.Colors.Reset} {trimmed.PadRight(width - 1)}{borderColor}\u2551{AppConstants.Colors.Reset}");
            }
            SafeConsoleWriteLine(bottom);
        }
        catch { _hasConsole = false; }
    }

    public static void DisplayBanner(string appName, string version, string author)
    {
        if (!_hasConsole) { SafeConsoleWriteLine($"{appName} v{version} by {author}"); return; }
        try
        {
            int w = SafeGetWindowWidth();
            string border = new string('\u2588', w);
            string emptyLine = "\u2588" + new string(' ', w - 2) + "\u2588";

            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{border}{AppConstants.Colors.Reset}");
            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{emptyLine}{AppConstants.Colors.Reset}");

            string title = appName;
            int titlePad = (w - 2 - title.Length) / 2;
            string titleLine = "\u2588" + new string(' ', titlePad) +
                $"{AppConstants.Colors.Bold}{AppConstants.Colors.White}{title}{AppConstants.Colors.Reset}{AppConstants.Colors.Cyan}" +
                new string(' ', w - 2 - titlePad - title.Length) + "\u2588";
            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{titleLine}{AppConstants.Colors.Reset}");

            string ver = $"Version {version}";
            int verPad = (w - 2 - ver.Length) / 2;
            string verLine = "\u2588" + new string(' ', verPad) +
                $"{AppConstants.Colors.Dim}{ver}{AppConstants.Colors.Reset}{AppConstants.Colors.Cyan}" +
                new string(' ', w - 2 - verPad - ver.Length) + "\u2588";
            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{verLine}{AppConstants.Colors.Reset}");

            string auth = $"Author: {author}";
            int authPad = (w - 2 - auth.Length) / 2;
            string authLine = "\u2588" + new string(' ', authPad) +
                $"{AppConstants.Colors.Dim}{auth}{AppConstants.Colors.Reset}{AppConstants.Colors.Cyan}" +
                new string(' ', w - 2 - authPad - auth.Length) + "\u2588";
            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{authLine}{AppConstants.Colors.Reset}");

            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{emptyLine}{AppConstants.Colors.Reset}");
            SafeConsoleWriteLine($"{AppConstants.Colors.Cyan}{border}{AppConstants.Colors.Reset}");
            SafeConsoleWriteLine("");
        }
        catch { _hasConsole = false; }
    }

    public static void DisplayNotification(string message, string type = "info")
    {
        if (!_hasConsole) { SafeConsoleWriteLine($"[{type}] {message}"); return; }
        try
        {
            string color = type switch
            {
                "success" => AppConstants.Colors.Success,
                "error" => AppConstants.Colors.Error,
                "warning" => AppConstants.Colors.Warning,
                _ => AppConstants.Colors.Info
            };
            string icon = type switch
            {
                "success" => AppConstants.Icons.Success,
                "error" => AppConstants.Icons.Error,
                "warning" => AppConstants.Icons.Warning,
                _ => AppConstants.Icons.Info
            };
            int w = SafeGetWindowWidth();
            string padded = $" {icon} {message} ";
            string border = $"{color}\u250C{new string('\u2500', Math.Min(padded.Length + 2, w - 2))}\u2510{AppConstants.Colors.Reset}";
            string line = $"{color}\u2502{AppConstants.Colors.Reset}{padded.PadRight(Math.Min(padded.Length + 2, w - 2) - 2)}{color}\u2502{AppConstants.Colors.Reset}";
            string bottom = $"{color}\u2514{new string('\u2500', Math.Min(padded.Length + 2, w - 2))}\u2518{AppConstants.Colors.Reset}";
            SafeConsoleWriteLine(border);
            SafeConsoleWriteLine(line);
            SafeConsoleWriteLine(bottom);
        }
        catch { _hasConsole = false; }
    }

    public static void WriteProgressBar(int percentage, int width = AppConstants.ProgressBarWidth)
    {
        if (!_hasConsole) return;
        try
        {
            int filled = percentage * width / 100;
            int empty = width - filled;
            string bar = $"{AppConstants.Colors.Success}{new string('\u2588', filled)}{AppConstants.Colors.Dim}{new string('\u2591', empty)}{AppConstants.Colors.Reset}";
            string pct = $"{AppConstants.Colors.Bold}{percentage,3}%{AppConstants.Colors.Reset}";
            Console.Write($"\r{bar} {pct}");
        }
        catch { _hasConsole = false; }
    }

    public static string PromptInput(string prompt, bool isPassword = false, string? defaultValue = null)
    {
        if (!_hasConsole) return defaultValue ?? "";
        try
        {
            Console.Write($" {AppConstants.Icons.Question} {prompt}: ");
            if (defaultValue != null) Console.Write($"[{defaultValue}] ");

            if (isPassword)
            {
                string password = "";
                while (true)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter) break;
                    if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                    {
                        password = password[..^1];
                        Console.Write("\b \b");
                    }
                    else if (key.Key != ConsoleKey.Backspace)
                    {
                        password += key.KeyChar;
                        Console.Write("*");
                    }
                }
                Console.WriteLine();
                return password;
            }

            string? input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) && defaultValue != null) return defaultValue;
            return input ?? "";
        }
        catch { _hasConsole = false; return defaultValue ?? ""; }
    }

    public static bool PromptConfirmation(string message)
    {
        if (!_hasConsole) return false;
        try
        {
            Console.Write($" {AppConstants.Icons.Question} {message} (y/n): ");
            var key = Console.ReadKey(true);
            Console.WriteLine(key.KeyChar);
            return key.Key == ConsoleKey.Y;
        }
        catch { _hasConsole = false; return false; }
    }

    public static ConsoleKey PromptKey(string message)
    {
        if (!_hasConsole) return ConsoleKey.Escape;
        try
        {
            Console.Write($" {AppConstants.Icons.Question} {message}");
            return Console.ReadKey(true).Key;
        }
        catch { _hasConsole = false; return ConsoleKey.Escape; }
    }

    public static void WaitForKey(string message = "Press any key to continue...")
    {
        if (!_hasConsole) return;
        try
        {
            Console.WriteLine();
            Console.Write($" {AppConstants.Icons.Info} {message}");
            Console.ReadKey(true);
            Console.WriteLine();
        }
        catch { _hasConsole = false; }
    }

    public static void CenterText(string text, string color = "")
    {
        if (string.IsNullOrEmpty(color)) color = AppConstants.Colors.Default;
        int w = SafeGetWindowWidth();
        int pad = (w - text.Length) / 2;
        SafeConsoleWriteLine($"{color}{new string(' ', Math.Max(0, pad))}{text}{AppConstants.Colors.Reset}");
    }
}
