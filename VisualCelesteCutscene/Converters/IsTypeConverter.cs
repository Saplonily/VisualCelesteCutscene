using System.Globalization;
using System.Windows.Data;

namespace VisualCelesteCutscene;

public sealed class IsTypeConverter : IValueConverter
{
    public Type? TargetType { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return TargetType is null ? false : (object)(value != null && TargetType.IsInstanceOfType(value));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}