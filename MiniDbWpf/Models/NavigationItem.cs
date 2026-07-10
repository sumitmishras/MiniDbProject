namespace MiniDbWpf.Models;

public class NavigationItem
{
    public string Title { get; set; } = string.Empty;
    public string IconKind { get; set; } = string.Empty;
    public Type PageType { get; set; } = null!;
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; } = true;
}
