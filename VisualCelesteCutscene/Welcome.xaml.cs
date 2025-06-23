using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace VisualCelesteCutscene;

public partial class Welcome : Window
{
    public Welcome()
    {
        InitializeComponent();
    }

    private void BtnOpenMod_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "目前程序正在处于极早期的开发中\n请在打开 Mod 之前务必备份该 Mod 或者仅打开用于测试的 Mod",
            "警告",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Exclamation
            );
        if (result is not MessageBoxResult.OK)
            return;

        OpenFileDialog ofd = new();
        ofd.Filter = "everest.yaml file (*.yaml)|*.yaml";
        if (ofd.ShowDialog() is true)
        {
            DialogEditor? win = null;
            try
            {
                CelesteMapMod? mapMod = CelesteMapMod.ReadFrom(Path.GetDirectoryName(ofd.FileName) ?? string.Empty) 
                    ?? throw new FormatException();
                DialogEditorViewModel viewModel = new(mapMod);
                win = new(viewModel);
                win.Show();
                Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
                win?.Close();
#if DEBUG
                if (Debugger.IsAttached)
                    throw;
#endif
            }
        }
    }

    private void BtnExit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BtnAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("这里暂时还没有关于(x");
    }

    private void BtnOptions_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("这里暂时还没有选项(x");
    }

    private void ListBoxRecentMods_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {

    }
}
