using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace VisualCelesteCutscene;

public sealed partial class TranslationEntryViewModel : ObservableObject
{
    [ObservableProperty]
    private string translation;

    public TranslationEntryViewModel(string translation)
    {
        this.translation = translation;
    }

    partial void OnTranslationChanged(string value)
    {
        App.Current.Messenger.Send<EntryChangedMessage>();
    }
}
