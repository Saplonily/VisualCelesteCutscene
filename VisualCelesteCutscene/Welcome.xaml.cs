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
        Loaded += Welcome_Loaded;
    }

    private void Welcome_Loaded(object sender, RoutedEventArgs e)
    {
        string testFile = @"C:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\MyFirstMod";

        DialogEditorWindow win = new(CelesteMapMod.ReadFrom(testFile)!);
        win.Show();
        Hide();
    }

    private void BtnOpenMod_Click(object sender, RoutedEventArgs e)
    {
#if RELEASE
        var result = MessageBox.Show(
            "目前程序正在处于极早期的开发中\n请在打开 Mod 之前务必备份该 Mod 或者仅打开用于测试的 Mod",
            "警告",
            MessageBoxButton.OKCancel,
            MessageBoxImage.Exclamation
            );
        if (result is not MessageBoxResult.OK)
            return;
#endif
        OpenFileDialog ofd = new();
        ofd.Filter = "everest.yaml file (*.yaml)|*.yaml";
        if (ofd.ShowDialog() is true)
        {
            DialogEditorWindow? win = null;
            try
            {
                win = OpenMod(ofd.FileName);
                Hide();
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "打开失败，请报告此问题", MessageBoxButton.OK, MessageBoxImage.Error);
                win?.Close();
#if DEBUG
                if (Debugger.IsAttached)
                    throw;
#endif
            }
        }
    }

    private static DialogEditorWindow OpenMod(string yamlPath)
    {
        CelesteMapMod? mapMod = CelesteMapMod.ReadFrom(Path.GetDirectoryName(yamlPath) ?? string.Empty)
            ?? throw new FormatException();

        return new DialogEditorWindow(mapMod);
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
