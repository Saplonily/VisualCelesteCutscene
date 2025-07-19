using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace VisualCelesteCutscene;

public partial class Welcome : Window, IWelcomeDialogHost
{
    private readonly WelcomeViewModel viewModel;

    public Welcome(UserData userData)
    {
        InitializeComponent();
        DataContext = viewModel = new WelcomeViewModel(this, userData);
    }

    private void BtnOpenMod_Click(object sender, RoutedEventArgs e)
    {
        string? file = RequestModFolder();
        if (file is null) return;
        viewModel.OpenMod.Execute(file);
    }

    private static DialogEditorWindow OpenMod(string modFolder)
    {
        CelesteMapMod? mapMod = CelesteMapMod.ReadFrom(modFolder ?? string.Empty)
            ?? throw new FormatException();

        return new DialogEditorWindow(mapMod);
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
        => Close();
    
    private void BtnAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("这里暂时还没有关于(x");
    }

    private void BtnOptions_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("这里暂时还没有选项(x");
    }

    private void ListBoxItemRecentMods_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        var item = ((ListBoxItem)sender).Content;
        if (item is null) return;
        viewModel.OpenMod.Execute(item);
    }

    private void ListBoxItemRecentMods_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter and not Key.Space)
            return;
        e.Handled = true;
        var item = ((ListBoxItem)sender).Content;
        if (item is null) return;
        viewModel.OpenMod.Execute(item);
    }

    public string? RequestModFolder()
    {
#if RELEASE
        var result = MessageBox.Show(
            "目前程序正在处于极早期的开发中\n请在打开 Mod 之前务必备份该 Mod 或者仅打开用于测试的 Mod",
            "警告",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Exclamation
            );
        if (result is not MessageBoxResult.OK)
            return null;
#endif
        OpenFileDialog ofd = new();
        ofd.Filter = "everest.yaml file (*.yaml)|*.yaml";
        if (ofd.ShowDialog() != true)
            return null;
        return Path.GetDirectoryName(ofd.FileName);
    }

    void IWelcomeDialogHost.ShowErrorDialog(string message, string title)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    void IWelcomeDialogHost.GotoEditor(string modFolder)
    {
        var win = OpenMod(modFolder);
        win.Show();
        Close();
    }
}
