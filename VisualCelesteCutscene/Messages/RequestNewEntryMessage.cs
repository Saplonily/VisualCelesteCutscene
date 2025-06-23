using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VisualCelesteCutscene;

public sealed class RequestNewEntryMessage : RequestMessage<(bool, string)?>
{
}
