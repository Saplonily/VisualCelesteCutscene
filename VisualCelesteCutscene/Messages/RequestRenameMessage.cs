using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VisualCelesteCutscene;

public sealed class RequestRenameMessage : RequestMessage<string?>
{
    public string OriginalName { get; set; }

    public RequestRenameMessage(string originalName)
    {
        OriginalName = originalName;
    }
}
