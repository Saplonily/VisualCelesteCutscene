using System.Diagnostics;

namespace CelesteDialog;

/// <summary>
/// A dialog plot page, for example:
/// <code>
/// [MADELINE left normal]
/// The sign out front is busted...{n}is this the {+mountain} trail?
/// 
/// [GRANNY right normal]
/// You're almost there.{n}It's just across the bridge.
/// </code>
/// </summary>
[DebuggerDisplay("{DebuggerInlinedText,nq}{Portrait}: {Text}")]
public sealed class DialogPlotPage : DialogPage
{
    public DialogPortraitState Portrait { get; set; }

    public string Text { get; set; }

    public bool InlinedToPrevious { get; set; }

    public DialogPlotPage(DialogPortraitState portrait, string text, bool inlinedToPrevious = false)
    {
        Portrait = portrait;
        Text = text;
        InlinedToPrevious = inlinedToPrevious;
    }

    private string DebuggerInlinedText => InlinedToPrevious ? "↑ " : "";

    public override DialogPlotPage Clone()
        => new(Portrait.Clone(), Text, InlinedToPrevious);
}
