namespace MiniDbProject.Models;

public class MenuItem
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ConsoleKey? ShortcutKey { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsSeparator { get; set; } = false;
    public string? Icon { get; set; }
}
