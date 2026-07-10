using MiniDbProject.Constants;

namespace MiniDbProject.Helpers;

public static class InputHelper
{
    public static string ReadString(string prompt, bool required = true, int maxLength = 100, string? defaultValue = null)
    {
        while (true)
        {
            string input = ConsoleHelper.PromptInput(prompt, defaultValue: defaultValue);
            if (!ConsoleHelper._hasConsole) return defaultValue ?? input;
            if (string.IsNullOrWhiteSpace(input) && required)
            {
                ConsoleHelper.WriteWarning("This field is required. Please enter a value.");
                continue;
            }
            if (input.Length > maxLength)
            {
                ConsoleHelper.WriteWarning($"Input too long. Maximum {maxLength} characters allowed.");
                continue;
            }
            return input;
        }
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue, int? defaultValue = null)
    {
        while (true)
        {
            string input = ConsoleHelper.PromptInput(prompt, defaultValue: defaultValue?.ToString());
            if (!ConsoleHelper._hasConsole) return defaultValue ?? min;
            if (string.IsNullOrWhiteSpace(input) && defaultValue.HasValue)
                return defaultValue.Value;

            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;

            ConsoleHelper.WriteWarning($"Please enter a valid number between {min} and {max}.");
        }
    }

    public static bool ReadBool(string prompt, bool? defaultValue = null)
    {
        return ConsoleHelper.PromptConfirmation(prompt);
    }

    public static ConsoleKey ReadKey(string prompt = "")
    {
        try
        {
            if (!string.IsNullOrEmpty(prompt))
                Console.Write($" {AppConstants.Icons.Question} {prompt}");
            return Console.ReadKey(true).Key;
        }
        catch { return ConsoleKey.Escape; }
    }

    public static string ReadPassword(string prompt)
    {
        return ConsoleHelper.PromptInput(prompt, isPassword: true);
    }

    public static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        ConsoleHelper.WriteLineColored($" {AppConstants.Icons.Question} {prompt}", AppConstants.Colors.Info);
        for (int i = 0; i < values.Length; i++)
        {
            ConsoleHelper.WriteLineColored($"   [{i + 1}] {values[i]}", AppConstants.Colors.Dim);
        }
        while (true)
        {
            try
            {
                Console.Write($" {AppConstants.Icons.Arrow} Select (1-{values.Length}): ");
                string? line = Console.ReadLine();
                if (!ConsoleHelper._hasConsole) return values[0];
                if (int.TryParse(line, out int choice) && choice >= 1 && choice <= values.Length)
                    return values[choice - 1];
            }
            catch { return values[0]; }
            ConsoleHelper.WriteWarning($"Please enter a number between 1 and {values.Length}.");
        }
    }
}
