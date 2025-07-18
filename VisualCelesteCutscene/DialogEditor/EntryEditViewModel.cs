using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualCelesteCutscene;

public abstract class EntryEditViewModel : ObservableObject
{
}

public sealed partial class PlotEntryEditViewModel : EntryEditViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<DialogPageViewModel> PagesViewModels { get; set; }

    public PlotEntryEditViewModel(ObservableCollection<DialogPageViewModel> pagesViewModels)
    {
        PagesViewModels = pagesViewModels;
    }

#if DEBUG
    public PlotEntryEditViewModel()
    {
        PagesViewModels = null!;
    }
#endif
}

public sealed partial class TranslationEntryEditViewModel : EntryEditViewModel
{
    [ObservableProperty]
    public partial string Translation { get; set; }

    public TranslationEntryEditViewModel(string translation)
    {
        Translation = translation;
    }
}