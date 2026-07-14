using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniDbWpf.Models;

public partial class NavigationItem : ObservableObject
{
    public string Title { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "•";
    public string IconKind { get; set; } = string.Empty;
    public Type PageType { get; set; } = null!;
    public string Subtitle { get; set; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;

    public bool IsVisible { get; set; } = true;
}
