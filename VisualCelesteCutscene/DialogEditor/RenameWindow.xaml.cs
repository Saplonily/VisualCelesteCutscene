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
        if (!DialogHelper.IsValidDialogKey(textBox.Text))
        {
            MessageBox.Show("项名称必须仅包含大小写字母，数字以及下划线。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        RenameResult = textBox.Text;
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        RenameResult = null;
        DialogResult = true;
    }
}