using System.Windows;
using Microsoft.Win32;

namespace VisualCelesteCutscene;

public sealed partial class SettingsWindow : Window
{
    private SettingsWindowViewModel viewModel;

    public SettingsWindow(UserData userData)
    {
        InitializeComponent();
        DataContext = viewModel = new SettingsWindowViewModel(userData);
    }

    private void ButtonGamePath_Click(object sender, RoutedEventArgs e)
    {
        string? f = RequestSelectFolder();
        if (f is not null)
            viewModel.CelesteGamePath = f;
    }

    private void ButtonDumpPath_Click(object sender, RoutedEventArgs e)
    {
        string? f = RequestSelectFolder();
        if (f is not null)
            viewModel.CelesteGraphicsDumpPath = f;
    }

    private string? RequestSelectFolder()
    {
        OpenFolderDialog openFolderDialog = new();
        bool? ret = openFolderDialog.ShowDialog(this);
        if (ret is true)
            return openFolderDialog.FolderName;
        else
            return null;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        viewModel.Dispose();
    }
}