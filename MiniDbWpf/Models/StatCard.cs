using System.Windows.Media;

namespace MiniDbWpf.Models;

public class StatCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string IconKind { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SolidColorBrush IconBrush { get; set; } = new SolidColorBrush(Color.FromRgb(88, 88, 232));
}
