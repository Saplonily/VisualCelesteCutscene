using System.Collections;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Celeste.Mod.VisualCelesteCutsceneHelper;

public sealed class VisualCelesteCutsceneHelperModule : EverestModule
{
    public static VisualCelesteCutsceneHelperModule Instance { get; private set; }

    public static RCEndPoint PreviewRCEndPoint { get; }

    private static readonly Regex insertRegex = new Regex("\\{\\+\\s*(.*?)\\}");

    static VisualCelesteCutsceneHelperModule()
    {
        PreviewRCEndPoint = new()
        {
            Path = "/vcc/preview",
            Name = "VCCPreviewInterface",
            InfoHTML = "<pre>Visual Celeste Cutscene</pre> preview interface.",
            Handle = HandleVCCPreview
        };
    }

    public override void Load()
    {
        Instance = this;
        Everest.DebugRC.EndPoints.Add(PreviewRCEndPoint);
    }

    public override void Unload()
    {
        Everest.DebugRC.EndPoints.Remove(PreviewRCEndPoint);
    }

    private static void HandleVCCPreview(HttpListenerContext c)
    {
        if (Engine.Scene is not Level level)
        {
            c.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            Everest.DebugRC.Write(c, "Not in level.");
            return;
        }
        Stream stream = c.Request.InputStream;
        StreamReader sr = new(stream, Encoding.UTF8);
        string str = sr.ReadToEnd();
        Everest.DebugRC.Write(c, "OK.");
        Logger.Log(LogLevel.Info, "VisualCelesteCutscene.Helper", $"Received preview request, length = {str.Length}.");

        MatchCollection matchCollection = null;
        while (matchCollection == null || matchCollection.Count > 0)
        {
            matchCollection = insertRegex.Matches(str);
            for (int i = 0; i < matchCollection.Count; i++)
            {
                Match match = matchCollection[i];
                string value = match.Groups[1].Value;
                str = Dialog.Language.Dialog.TryGetValue(value, out string refered)
                    ? str.Replace(match.Value, refered)
                    : str.Replace(match.Value, $"[{value}]");
            }
        }

        Dialog.Language.Dialog["vcc_preview"] = str;

        EndCutscene(level);

        level.StartCutscene(EndCutscene);
        Player player = level.Tracker.GetEntity<Player>();
        Entity cutsceneEntity = new();
        cutsceneEntity.Add(new Coroutine(Routine(level, player)));
        level.Add(cutsceneEntity);

        static void EndCutscene(Level level)
        {
            foreach (var textbox in level.Tracker.GetEntities<Textbox>())
                ((Textbox)textbox).Close();
            level.EndCutscene();
            level.Tracker.GetEntity<Player>().StateMachine.State = Player.StNormal;
        }

        static IEnumerator Routine(Level level, Player player)
        {
            yield return null;
            player.StateMachine.State = Player.StDummy;
            yield return Textbox.Say("vcc_preview");
            player.StateMachine.State = Player.StNormal;
        }
    }
}