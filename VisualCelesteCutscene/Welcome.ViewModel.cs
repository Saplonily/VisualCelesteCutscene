using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VisualCelesteCutscene;

public sealed class WelcomeViewModel : ObservableObject
{
    private readonly IWelcomeDialogHost welcomeDialogHost;

    public static string Version => App.Current.Version;

    public RelayCommand<string> OpenMod { get; }

    public IReadOnlyList<string> RecentOpenedModFolders { get; }

    private readonly UserData userData;

    public WelcomeViewModel(IWelcomeDialogHost welcomeDialogHost, UserData userData)
    {
        this.welcomeDialogHost = welcomeDialogHost;
        this.userData = userData;

        OpenMod = new(OnOpenMod!);
        RecentOpenedModFolders = userData.RecentMods;
    }

    private void OnOpenMod(string modFolder)
    {
        welcomeDialogHost.GotoEditor(modFolder);
        var recentMods = userData.RecentMods;

        int index = -1;
        for (var i = 0; i < recentMods.Count; i++)
        {
            if (PathEqual(recentMods[i], modFolder))
            {
                index = i;
                break;
            }
        }

        if (index != -1)
            recentMods.RemoveAt(index);

        recentMods.Insert(0, modFolder);
    }

    private static bool PathEqual(string pathA, string pathB)
    {
        string fullA = Path.GetFullPath(pathA);
        string fullB = Path.GetFullPath(pathB);
        if (OperatingSystem.IsWindows())
            return fullA.Equals(fullB, StringComparison.OrdinalIgnoreCase);
        else
            return fullA == fullB;
    }
}
