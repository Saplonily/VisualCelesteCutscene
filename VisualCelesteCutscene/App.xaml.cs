using System.IO;
using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public string Version { get; } = typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    public PortraitsImageService PortraitsInfoService { get; }

    public PreviewService PreviewService { get; }

    public DialogFileService DialogFileService { get; }

    public WeakReferenceMessenger Messenger { get; }

    public Welcome WelcomeWindow { get; }

    public App()
    {
        DialogFileService = new();
        PortraitsInfoService = new();
        try
        {
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
        catch (Exception e)
        {
            File.WriteAllText("log-vcc.txt", e.ToString());
            throw;
        }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        WelcomeWindow.Show();
    }
}
