using System.Windows.Media;

namespace MiniDbWpf.Models;

public class StatCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string IconGlyph { get; set; } = "📊";
    public string IconKind { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SolidColorBrush AccentBrush { get; set; } = new(Color.FromRgb(124, 58, 237));
    public SolidColorBrush IconBrush { get; set; } = new(Color.FromRgb(124, 58, 237));
}
