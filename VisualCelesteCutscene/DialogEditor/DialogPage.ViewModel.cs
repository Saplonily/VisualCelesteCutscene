using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Cele = CelesteDialog;

namespace VisualCelesteCutscene;

public abstract partial class DialogPageViewModel : ObservableObject, ICloneable
{
    [ObservableProperty]
    public partial int Index { get; set; }

    public void ListenOnChanged()
        => PropertyChanged += static (_, args) =>
        {
            // any better way?
            if (
                args.PropertyName is
                not (nameof(DialogPlotPageViewModel.SelectionStart)) and
                not (nameof(DialogPlotPageViewModel.SelectionLength))
            )
            {
                App.Current.Messenger.Send<EntryChangedMessage>();
            }
        };

    public abstract DialogPageViewModel Clone();

    object ICloneable.Clone()
        => Clone();

    public abstract Cele.DialogPage ToModel();
}