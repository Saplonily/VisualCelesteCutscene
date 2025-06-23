using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualCelesteCutscene;

public partial class DialogPage : UserControl
{
    public DialogPage()
    {
        InitializeComponent();
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        TextBox box = (TextBox)sender;
        if (box.LineCount > 3)
        {
            string str = box.Text;
            var newLinePos = str.IndexOf('\n', str.IndexOf('\n', str.IndexOf('\n') + 1) + 1);
            box.Text = str[..(newLinePos - 1)];
        }
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox box = (TextBox)sender;
        if (e.Key is Key.Enter && box.LineCount >= 3)
            e.Handled = true;
    }
}