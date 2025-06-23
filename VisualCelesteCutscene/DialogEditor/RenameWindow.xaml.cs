using System.Windows;

namespace VisualCelesteCutscene;

public partial class RenameWindow : Window
{
    public string? RenameResult { get; private set; }
    
    public RenameWindow(string original)
    {
        InitializeComponent();
        textBox.Text = original;
        textBox.Focus();
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        RenameResult = textBox.Text;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        RenameResult = null;
        DialogResult = true;
    }
}