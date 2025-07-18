using System.Windows;
using System.Windows.Controls;

namespace VisualCelesteCutscene;

public partial class DialogEditView : UserControl
{
    public DialogEditView()
    {
        InitializeComponent();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("别点了这个功能还没做(x");
    }

    private void ListBoxItem_GotFocus(object sender, RoutedEventArgs e)
    {
        ((ListBoxItem)sender).IsSelected = true;
    }
}
