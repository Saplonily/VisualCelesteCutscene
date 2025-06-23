using System.Windows;
using System.Windows.Media;

namespace VisualCelesteCutscene;

public partial class ColorPickerWindow : Window
{
    public Color Color { get; set; }

    public ColorPickerWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        Color = colorPicker.SelectedColor;
        DialogResult = true;
    }
}
