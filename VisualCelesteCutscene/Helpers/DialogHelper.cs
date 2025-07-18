namespace VisualCelesteCutscene;

public static class DialogHelper
{
    public static string DialogKeyify(string str) 
        => str.Replace('/', '_').Replace('-', '_').Replace('+', '_').Replace(' ', '_');

    public static bool IsValidDialogKey(string key)
        => key != string.Empty && key.All(c => char.IsAsciiLetterOrDigit(c) || c is '_');
}