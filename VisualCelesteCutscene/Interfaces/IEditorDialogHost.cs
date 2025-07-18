using System.Windows.Media;

namespace VisualCelesteCutscene;

public interface IEditorDialogHost
{
    public Color? RequestColor();

    public void ShowErrorDialog(string message, string title);

    public bool RequestConfirm(string message, string title);
}