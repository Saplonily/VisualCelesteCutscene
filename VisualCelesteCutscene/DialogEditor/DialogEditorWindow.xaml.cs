using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CelesteDialog;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public partial class DialogEditorWindow : Window,
    IRecipient<RequestNewPageMessage>,
    IRecipient<RequestNewEntryMessage>,
    IRecipient<RequestRenameMessage>,
    IEditorDialogHost
{
    private bool gotoWelcomeOnClosed = true;
    private readonly DialogEditorWindowViewModel viewModel;

    public DialogEditorWindow(CelesteMapMod celesteMapMod)
    {
        InitializeComponent();
        DataContext = viewModel = new(celesteMapMod, this);

        var messenger = App.Current.Messenger;
        messenger.Register<RequestNewPageMessage>(this);
        messenger.Register<RequestNewEntryMessage>(this);
        messenger.Register<RequestRenameMessage>(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        App.Current.Messenger.UnregisterAll(this);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        if (viewModel.Edits.Any(e => e.Edit.IsDirty))
        {
            var result = MessageBox.Show(
                this,
                "是否保存对当前文件所做的更改？",
                string.Empty,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question
                );
            switch (result)
            {
            case MessageBoxResult.Yes:
                viewModel.SaveAllCommand.Execute(null);
                break;
            case MessageBoxResult.No:
                break;
            case MessageBoxResult.Cancel:
                e.Cancel = true;
                return;
            }
        }
        if (gotoWelcomeOnClosed)
            App.Current.BackToWelcome();
    }

    void IRecipient<RequestNewPageMessage>.Receive(RequestNewPageMessage message)
    {
        NewPageWindow newPageWindow = new(message.AllowRelative)
        {
            ShowInTaskbar = false,
            Owner = this
        };
        if (newPageWindow.ShowDialog() is true)
            message.Reply((newPageWindow.PageNewPosition, newPageWindow.DialogPageType));
        else
            message.Reply(null);
    }

    bool IEditorDialogHost.RequestConfirm(string message, string title)
        => MessageBox.Show(this, message, title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes;

    Color? IEditorDialogHost.RequestColor()
    {
        ColorPickerWindow win = new()
        {
            ShowInTaskbar = false,
            Owner = this
        };
        return win.ShowDialog() is true ? win.Color : null;
    }

    void IEditorDialogHost.ShowErrorDialog(string message, string title)
    {
        MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    void IRecipient<RequestNewEntryMessage>.Receive(RequestNewEntryMessage message)
    {
        NewEntryWindow win = new()
        {
            ShowInTaskbar = false,
            Owner = this
        };
        if (win.ShowDialog() is true)
            message.Reply((win.IsPlotEntry, win.EntryName!));
        else
            message.Reply(null);
    }

    void IRecipient<RequestRenameMessage>.Receive(RequestRenameMessage message)
    {
        RenameWindow win = new(message.OriginalName)
        {
            ShowInTaskbar = false,
            Owner = this,
        };
        if (win.ShowDialog() is true)
            message.Reply(win.RenameResult);
        else
            message.Reply(null);
    }

    private void MenuItemClose_Click(object sender, RoutedEventArgs e)
    {
        gotoWelcomeOnClosed = true;
        Close();
    }

    private void MenuItemExit_Click(object sender, RoutedEventArgs e)
    {
        gotoWelcomeOnClosed = false;
        Close();
    }
}