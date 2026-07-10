using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MiniDbWpf.Converters;

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Info" => new SolidColorBrush(Color.FromRgb(100, 149, 237)),
            "Success" => new SolidColorBrush(Color.FromRgb(80, 200, 120)),
            "Warning" => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            "Error" => new SolidColorBrush(Color.FromRgb(255, 82, 82)),
            _ => new SolidColorBrush(Color.FromRgb(200, 200, 200))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
