namespace VisualCelesteCutscene;

public sealed class UserData
{
    public string CelesteGamePath { get; set; }

    public string CelesteGraphicsDumpPath { get; set; }

    public List<string> RecentMods { get; set; }

    public UserData()
    {
        CelesteGamePath = string.Empty;
        CelesteGraphicsDumpPath = string.Empty;
        RecentMods = new();
    }
}
