using System.Collections.ObjectModel;

namespace VisualCelesteCutscene;

// TODO implement this
// damn wpf, why there's alternation index but no just index

public sealed class IndexedObservableCollection<T> : ObservableCollection<IndexedItem<T>>
{
    
}

public record struct IndexedItem<T>(int Index, T Item);