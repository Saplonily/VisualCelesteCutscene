using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CelesteDialog;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public partial class DialogEditor : Window,
    IRecipient<RequestNewPageMessage>,
    IRecipient<RequestConfirmMessage>,
    IRecipient<RequestColorPickMessage>,
    IRecipient<RequestNewEntryMessage>,
    IRecipient<RequestRenameMessage>
{
    private bool gotoWelcomeOnClosed = true;
    private readonly DialogEditorViewModel viewModel;

    public DialogEditor(DialogEditorViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
        viewModel.RequestClose += ViewModel_RequestClose;
        viewModel.RequestExit += ViewModel_RequestExit;

        var messenger = App.Current.Messenger;
        messenger.Register<RequestNewPageMessage>(this);
        messenger.Register<RequestConfirmMessage>(this);
        messenger.Register<RequestColorPickMessage>(this);
        messenger.Register<RequestNewEntryMessage>(this);
        messenger.Register<RequestRenameMessage>(this);
        this.viewModel = viewModel;
    }

    private void ViewModel_RequestExit()
    {
        gotoWelcomeOnClosed = false;
        Close();
    }

    private void ViewModel_RequestClose()
    {
        gotoWelcomeOnClosed = true;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (gotoWelcomeOnClosed)
            App.Current.WelcomeWindow.Show();
        else
            App.Current.WelcomeWindow.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (viewModel.EntriesDirty)
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
                break;
            }
        }
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

    void IRecipient<RequestConfirmMessage>.Receive(RequestConfirmMessage message)
    {
        MessageBoxResult result = MessageBox.Show(message.Message, message.Title, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
        message.Reply(result is MessageBoxResult.Yes);
    }

    void IRecipient<RequestColorPickMessage>.Receive(RequestColorPickMessage message)
    {
        ColorPickerWindow win = new()
        {
            ShowInTaskbar = false,
            Owner = this
        };
        if (win.ShowDialog() is true)
            message.Reply(win.Color);
        else
            message.Reply(null);
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

    private void ListBoxItem_GotFocus(object sender, RoutedEventArgs e)
    {
        ((ListBoxItem)sender).IsSelected = true;
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

    private void DialogTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        App.Current.Messenger.Send<EntryChangedMessage>();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("别点了这个功能还没做(x");
    }
}
