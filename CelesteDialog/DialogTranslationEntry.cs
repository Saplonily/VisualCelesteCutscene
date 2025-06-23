using System.Diagnostics;

namespace CelesteDialog;

/// <summary>
/// A dialog translation entry. Entry without character "[" and "]"
/// will be seen as a translation entry.
/// </summary>
[DebuggerDisplay("{Translation}")]
public sealed class DialogTranslationEntry : DialogEntry, ICloneable
{
    public string Translation { get; set; }

    public DialogTranslationEntry()
        => Translation = string.Empty;

    public DialogTranslationEntry(string translation)
    {
        Translation = translation;
    }

    public override DialogTranslationEntry Clone()
        => new(Translation);

    object ICloneable.Clone()
        => Clone();
}
