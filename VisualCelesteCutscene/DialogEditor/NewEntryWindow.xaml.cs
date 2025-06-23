using System.Windows;

namespace VisualCelesteCutscene;

public partial class NewEntryWindow : Window
{
    public bool IsPlotEntry { get; private set; }

    public string? EntryName { get; private set; }

    public NewEntryWindow()
    {
        InitializeComponent();
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        IsPlotEntry = btnTrans.IsChecked is true;
        EntryName = textBoxName.Text;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
