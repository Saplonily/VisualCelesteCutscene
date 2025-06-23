using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CelesteDialog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualCelesteCutscene;

/// <summary>
/// (<see cref="string"/> EntryName, <see cref="DialogEntry"/> Entry, <see cref="bool"/> Dirty)
/// </summary>
[DebuggerDisplay("{EntryName}, IsDirty = {IsDirty}")]
public sealed partial class EntryViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string EntryName { get; set; }

    [ObservableProperty]
    public partial DialogEntry Entry { get; set; }

    [ObservableProperty]
    public partial bool IsDirty { get; set; }

    public EntryViewModel(string entryName, DialogEntry entry)
    {
        EntryName = entryName;
        Entry = entry;
    }
}