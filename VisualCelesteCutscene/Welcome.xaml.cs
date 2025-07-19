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
    private readonly UserData userData;

    public Welcome(UserData userData)
    {
        InitializeComponent();
        DataContext = viewModel = new WelcomeViewModel(this, userData);
        this.userData = userData;
        ContentRendered += Welcome_ContentRendered;
    }

    private void Welcome_ContentRendered(object? sender, EventArgs e)
    {
        if (!userData.IsNotFirstTime)
        {
            MessageBox.Show("""
                欢迎来到 Visual Celeste Cutscene 系列之
                - Visual Celeste Dialog Editor
                一个可视化的蔚蓝剧情文件的编辑器
                (一个因为 GUI 编程能力不足耗费我脑细胞的东西)

                要开始使用的话，记得先前往设置配置你的蔚蓝路径和 Graphics Dump 路径
                """, "欢迎", MessageBoxButton.OK
                );
            userData.IsNotFirstTime = true;
            App.Current.SaveUserData();
        }
    }

    private void BtnOpenMod_Click(object sender, RoutedEventArgs e)
    {
        if (!CheckAndWarnPath()) return;
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

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        new SettingsWindow(userData)
        {
            Owner = this,
            ShowInTaskbar = false
        }.ShowDialog();
    }

    private void ListBoxItemRecentMods_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (!CheckAndWarnPath()) return;
        var item = ((ListBoxItem)sender).Content;
        if (item is null) return;
        viewModel.OpenMod.Execute(item);
    }

    private void ListBoxItemRecentMods_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Enter and not Key.Space)
            return;
        e.Handled = true;
        if (!CheckAndWarnPath()) return;
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

    private bool CheckAndWarnPath()
    {
        if (!userData.CheckPathValid())
        {
            MessageBox.Show("设置中配置的路径存在问题，请前往设置窗口查看。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        return true;
    }
}
