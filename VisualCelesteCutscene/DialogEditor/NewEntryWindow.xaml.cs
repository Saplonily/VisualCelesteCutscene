using System.Windows;

namespace VisualCelesteCutscene;

public partial class NewEntryWindow : Window
{
    public bool IsPlotEntry { get; private set; }

    public string? EntryName { get; private set; }

    public NewEntryWindow()
    {
        InitializeComponent();
        textBoxName.Focus();
    }

    private void BtnOK_Click(object sender, RoutedEventArgs e)
    {
        if (!DialogHelper.IsValidDialogKey(textBoxName.Text))
        {
            MessageBox.Show("项名称必须仅包含大小写字母，数字以及下划线。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        IsPlotEntry = btnTrans.IsChecked is true;
        EntryName = textBoxName.Text;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
