using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Saladim.Blockly;

public partial class BlocklyBlock : UserControl
{
    private Point? startPosition;

    [MemberNotNullWhen(true, nameof(startPosition))]
    private bool IsDragging => startPosition is not null;

    public BlocklyBlock()
    {
        InitializeComponent();
        PreviewMouseLeftButtonDown += BlockControl_PreviewMouseLeftButtonDown;
        MouseMove += BlockControl_MouseMove;
        PreviewMouseLeftButtonUp += BlockControl_PreviewMouseLeftButtonUp;
    }

    private void BlockControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            startPosition = e.GetPosition(this);
            CaptureMouse();
            e.Handled = true;
        }
    }

    private void BlockControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (IsDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPosition = e.GetPosition(Parent as UIElement);

            double newX = currentPosition.X - startPosition.Value.X;
            double newY = currentPosition.Y - startPosition.Value.Y;

            if (Parent is Canvas parentCanvas)
            {
                newX = Math.Clamp(newX, 0, parentCanvas.ActualWidth - ActualWidth);
                newY = Math.Clamp(newY, 0, parentCanvas.ActualHeight - ActualHeight);

                Canvas.SetLeft(this, newX);
                Canvas.SetTop(this, newY);
            }
            e.Handled = true;
        }
    }

    private void BlockControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (IsDragging)
        {
            startPosition = null;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}
