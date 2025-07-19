using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    public string Version { get; } = typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    // TODO move away these services
    public PortraitsImageService PortraitsInfoService { get; }

    public PreviewService PreviewService { get; }

    public DialogFileService DialogFileService { get; }

    public WeakReferenceMessenger Messenger { get; }

    public UserData UserData { get; set; }

    private string SaveFile;

    public App()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData,
                Environment.SpecialFolderOption.Create
                ),
            nameof(VisualCelesteCutscene)
            );
        Directory.CreateDirectory(path);
        SaveFile = Path.Combine(path, "save.json");

        UserData = LoadUserData();

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
        }
        catch (Exception e)
        {
            File.WriteAllText("log-vcc.txt", e.ToString());
            throw;
        }
    }

    public void BackToWelcome()
    {
        Welcome welcome = new(UserData);
        welcome.Show();
    }

    public void LeaveFromWelcome(string yamlPath)
    {
        SaveUserData(UserData);
        var win = OpenMod(yamlPath);
        win.Show();
    }

    private static DialogEditorWindow OpenMod(string yamlPath)
    {
        CelesteMapMod? mapMod = CelesteMapMod.ReadFrom(Path.GetDirectoryName(yamlPath) ?? string.Empty)
            ?? throw new FormatException();

        return new DialogEditorWindow(mapMod);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        BackToWelcome();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        SaveUserData(UserData);
    }

    private UserData LoadUserData()
    {
        if (!File.Exists(SaveFile)) return new UserData();
        using (FileStream fs = new(SaveFile, FileMode.Open, FileAccess.Read))
        {
            return JsonSerializer.Deserialize<UserData>(fs) ?? new();
        }
    }

    private void SaveUserData(UserData userData)
    {
        using (FileStream fs = new(SaveFile, FileMode.Create, FileAccess.Write))
        {
            JsonSerializer.Serialize(fs, userData);
        }
    }
}
