using System.Windows;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public PortraitsImageService PortraitsInfoService { get; }

    public PreviewService PreviewService { get; }

    public DialogFileService DialogFileService { get; }

    public WeakReferenceMessenger Messenger { get; }

    public Welcome WelcomeWindow { get; }

    public App()
    {
        DialogFileService = new();
        PortraitsInfoService = new();
        PortraitsInfoService.AddPortraitsSource(
            @"C:\Program Files (x86)\Steam\steamapps\common\Celeste\Content\Graphics\Portraits.xml"
        );
        PortraitsInfoService.AddImageSearchPath(
            @"C:\Program Files (x86)\Steam\steamapps\common\Celeste\Celeste Graphics Dump v1400\Portraits"
        );
        Messenger = WeakReferenceMessenger.Default;
        PreviewService = new();

        WelcomeWindow = new();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        CelesteMapMod? mod = CelesteMapMod.ReadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Celeste\Mods\ChineseNewYear2024Collab v2.0.14");
        WelcomeWindow.Show();
    }
}
