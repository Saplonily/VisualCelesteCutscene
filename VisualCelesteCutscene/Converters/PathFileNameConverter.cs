using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace VisualCelesteCutscene;

public sealed class PathFileNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Path.GetFileName((string)value);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}