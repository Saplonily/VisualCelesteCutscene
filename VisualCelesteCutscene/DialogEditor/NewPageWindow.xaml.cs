using System.Windows;

namespace VisualCelesteCutscene;

public partial class NewPageWindow : Window
{
    public bool AllowRelative { get; private set; }

    public PageNewPosition PageNewPosition { get; private set; }

    public DialogPageType DialogPageType { get; private set; }

    public NewPageWindow(bool allowRelative)
    {
        InitializeComponent();
        AllowRelative = allowRelative;
        DataContext = this;
    }

    private void AddToTop_Click(object sender, RoutedEventArgs e)
    {
        GetPageType();
        PageNewPosition = PageNewPosition.Top;
        DialogResult = true;
    }

    private void InsertItemAbove_Click(object sender, RoutedEventArgs e)
    {
        GetPageType();
        PageNewPosition = PageNewPosition.Above;
        DialogResult = true;
    }

    private void InsertItemBelow_Click(object sender, RoutedEventArgs e)
    {
        GetPageType();
        PageNewPosition = PageNewPosition.Below;
        DialogResult = true;
    }

    private void AddToBottom_Click(object sender, RoutedEventArgs e)
    {
        GetPageType();
        PageNewPosition = PageNewPosition.Bottom;
        DialogResult = true;
    }

    private void GetPageType()
    {
        DialogPageType = rbPlot.IsChecked is true ? DialogPageType.Plot
            : rbInlinedPlot.IsChecked is true ? DialogPageType.InlinedPlot
            : DialogPageType.Invalid;
    }
}
