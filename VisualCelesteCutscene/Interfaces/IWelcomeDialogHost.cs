namespace VisualCelesteCutscene;

public interface IWelcomeDialogHost
{
    public string? RequestModFolder();

    public void ShowErrorDialog(string message, string title);

    public void GotoEditor(string modFolder);
}