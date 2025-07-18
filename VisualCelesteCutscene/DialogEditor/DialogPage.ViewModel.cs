using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Cele = CelesteDialog;

namespace VisualCelesteCutscene;

public abstract partial class DialogPageViewModel : ObservableObject, ICloneable
{
    // TODO put it here is bad, remove it
    [ObservableProperty]
    public partial int Index { get; set; }

    public event Action? Dirty;

    public void ListenOnChanged()
        => PropertyChanged += (_, args) =>
        {
            // TODO same as the above todo
            if (
                args.PropertyName is
                not (nameof(DialogPlotPageViewModel.SelectionStart)) and
                not (nameof(DialogPlotPageViewModel.SelectionLength))
            )
            {
                Dirty?.Invoke();
            }
        };

    public abstract DialogPageViewModel Clone();

    object ICloneable.Clone()
        => Clone();

    public abstract Cele.DialogPage ToModel();
}