namespace CelesteDialog;

/// <summary>
/// A dialog plot entry, containing a list of <see cref="DialogPage"/>.
/// Entry with character "[" and "]" will be seen as a plot entry.
/// </summary>
public sealed class DialogPlotEntry : DialogEntry
{
    public List<DialogPage> Pages { get; set; }

    public DialogPlotEntry()
        => Pages = new();

    public DialogPlotEntry(IEnumerable<DialogPage> pages)
        => Pages = pages.ToList();

    public override DialogPlotEntry Clone()
        => new(Pages.Select(p => p.Clone()));
}
