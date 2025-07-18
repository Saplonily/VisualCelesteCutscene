using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisualCelesteCutscene;

public sealed class DirtyToItalicConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => ((bool)value) ? FontStyles.Italic : FontStyles.Normal;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}