using CommunityToolkit.Mvvm.Messaging.Messages;

namespace VisualCelesteCutscene;

public sealed class RequestConfirmMessage : RequestMessage<bool>
{
    public string Title { get; set; }

    public string Message { get; set; }

    public RequestConfirmMessage(string title, string message)
    {
        Title = title;
        Message = message;
    }
}
