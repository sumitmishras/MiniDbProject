namespace MiniDbProject.Constants;

public static class AppConstants
{
    public const string AppName = "Mini Database Project";
    public const string AppVersion = "1.0.0";
    public const string Author = "Your Name";
    public const string FrameworkVersion = "NET 10.0";

    public const int DefaultConsoleWidth = 120;
    public const int DefaultConsoleHeight = 40;
    public const int ProgressBarWidth = 50;

    public static class Colors
    {
        public const string Default = "\x1b[37m";
        public const string Primary = "\x1b[36m";
        public const string Success = "\x1b[32m";
        public const string Warning = "\x1b[33m";
        public const string Error = "\x1b[31m";
        public const string Info = "\x1b[34m";
        public const string Header = "\x1b[35m";
        public const string Highlight = "\x1b[93m";
        public const string Dim = "\x1b[2m";
        public const string Reset = "\x1b[0m";
        public const string White = "\x1b[97m";
        public const string Cyan = "\x1b[96m";
        public const string Green = "\x1b[92m";
        public const string Yellow = "\x1b[93m";
        public const string Red = "\x1b[91m";
        public const string Blue = "\x1b[94m";
        public const string Magenta = "\x1b[95m";
        public const string BgDark = "\x1b[40m";
        public const string BgBlue = "\x1b[44m";
        public const string BgGreen = "\x1b[42m";
        public const string BgRed = "\x1b[41m";
        public const string BgYellow = "\x1b[43m";
        public const string Bold = "\x1b[1m";
        public const string Underline = "\x1b[4m";
    }

    public static class Icons
    {
        public const string Success = "[+]";
        public const string Error = "[!]";
        public const string Warning = "[*]";
        public const string Info = "[i]";
        public const string Question = "[?]";
        public const string Arrow = "->";
        public const string Star = "*";
        public const string Check = "ok";
        public const string Cross = "xx";
        public const string Progress = "###";
        public const string Lock = "[LOCK]";
        public const string User = "[USER]";
        public const string Database = "[DB]";
        public const string Server = "[SRV]";
        public const string Log = "[LOG]";
        public const string Settings = "[SET]";
        public const string Help = "[HELP]";
        public const string Home = "[HOME]";
        public const string Exit = "[EXIT]";
        public const string Back = "[BACK]";
    }

    public static class MenuKeys
    {
        public const ConsoleKey Exit = ConsoleKey.Escape;
        public const ConsoleKey Back = ConsoleKey.B;
        public const ConsoleKey Confirm = ConsoleKey.Enter;
        public const ConsoleKey Cancel = ConsoleKey.C;
        public const ConsoleKey Pause = ConsoleKey.P;
        public const ConsoleKey Resume = ConsoleKey.R;
        public const ConsoleKey Refresh = ConsoleKey.F5;
        public const ConsoleKey Help = ConsoleKey.F1;
        public const ConsoleKey Search = ConsoleKey.F;
        public const ConsoleKey Export = ConsoleKey.E;
        public const ConsoleKey Up = ConsoleKey.UpArrow;
        public const ConsoleKey Down = ConsoleKey.DownArrow;
        public const ConsoleKey Select = ConsoleKey.Enter;
    }

    public static class LogLevels
    {
        public const string Information = "INFORMATION";
        public const string Warning = "WARNING";
        public const string Error = "ERROR";
        public const string Success = "SUCCESS";
        public const string Debug = "DEBUG";
    }
}
