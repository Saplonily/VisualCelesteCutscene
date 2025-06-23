using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VisualCelesteCutscene;

public sealed class RequestNewPageMessage : RequestMessage<(PageNewPosition, DialogPageType)?>
{
    public bool AllowRelative { get; set; }

    public RequestNewPageMessage(bool allowRelative)
    {
        AllowRelative = allowRelative;
    }
}
