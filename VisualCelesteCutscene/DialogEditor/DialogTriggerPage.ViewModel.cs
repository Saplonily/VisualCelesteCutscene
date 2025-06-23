using CelesteDialog;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisualCelesteCutscene;

public sealed partial class DialogTriggerPageViewModel : DialogPageViewModel
{
    [ObservableProperty]
    private string triggerName;

    [ObservableProperty]
    public partial string ArgString { get; set; }

    // make designer happy
#if DEBUG
    public DialogTriggerPageViewModel()
    {
        triggerName = "Example TriggerName";
        ArgString = "Example Arg1 Arg2";
    }
#endif

    public DialogTriggerPageViewModel(DialogTriggerPage page)
    {
        triggerName = page.TriggerName;
        ArgString = string.Join(' ', page.Arguments);
    }

    public override DialogTriggerPageViewModel Clone()
        => new(new DialogTriggerPage(TriggerName, ArgString.Split(' ')));

    public override DialogTriggerPage ToModel()
        => new DialogTriggerPage(TriggerName, ArgString.Split(' '));
}
