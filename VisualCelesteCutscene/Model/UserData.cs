namespace VisualCelesteCutscene;

public sealed class UserData
{
    public string CelesteGamePath { get; set; }

    public string CelesteGraphicsDumpPath { get; set; }

    public List<string> RecentMods { get; set; }

    public bool IsNotFirstTime { get; set; }

    public UserData()
    {
        CelesteGamePath = string.Empty;
        CelesteGraphicsDumpPath = string.Empty;
        RecentMods = new();
    }

    public bool CheckPathValid()
    {
        if (PathValidator.ValidateCelesteGamePath(CelesteGamePath) != string.Empty)
            return false;
        if (PathValidator.ValidateCelesteGraphicsDumpPath(CelesteGraphicsDumpPath) != string.Empty)
            return false;
        return true;
    }
}
