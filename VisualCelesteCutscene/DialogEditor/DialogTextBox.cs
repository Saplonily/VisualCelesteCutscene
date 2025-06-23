using System.Windows;
using System.Windows.Controls;

namespace VisualCelesteCutscene;

public sealed class DialogTextBox : TextBox
{
    public static readonly DependencyProperty BindableSelectionStartProperty =
        DependencyProperty.Register(nameof(BindableSelectionStart), typeof(int), typeof(DialogTextBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectionStartChanged)
            );

    public static readonly DependencyProperty BindableSelectionLengthProperty =
        DependencyProperty.Register(nameof(BindableSelectionLength), typeof(int), typeof(DialogTextBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBindableSelectionLengthChanged)
            );

    public int BindableSelectionStart
    {
        get => (int)GetValue(BindableSelectionStartProperty);
        set => SetValue(BindableSelectionStartProperty, value);
    }

    public int BindableSelectionLength
    {
        get => (int)GetValue(BindableSelectionLengthProperty);
        set => SetValue(BindableSelectionLengthProperty, value);
    }

    public DialogTextBox()
    {
        SelectionChanged += BindableTextBox_SelectionChanged;
    }

    private static void OnBindableSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        DialogTextBox self = (DialogTextBox)d;
        if (self.SelectionStart != (int)e.NewValue)
            self.SelectionStart = (int)e.NewValue;
    }

    private static void OnBindableSelectionLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        DialogTextBox self = (DialogTextBox)d;
        if (self.SelectionLength != (int)e.NewValue)
            self.SelectionLength = (int)e.NewValue;
    }

    private void BindableTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        SetCurrentValue(BindableSelectionStartProperty, SelectionStart);
        SetCurrentValue(BindableSelectionLengthProperty, SelectionLength);
    }
}
