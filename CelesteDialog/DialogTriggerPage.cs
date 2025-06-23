using System.Diagnostics;

namespace CelesteDialog;

/// <summary>
/// A dialog trigger page, containing a trigger. Such as <c>{trigger 0 Madeline walks forward}</c>.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class DialogTriggerPage : DialogPage
{
    public string TriggerName { get; set; }

    public List<string> Arguments { get; set; }

    public DialogTriggerPage(string triggerName, IEnumerable<string> arguments)
    {
        TriggerName = triggerName;
        Arguments = arguments.ToList();
    }

    private string DebuggerDisplay
    {
        get => "{" + TriggerName + " " + string.Join(' ', Arguments) + "}";
    }

    public override DialogTriggerPage Clone()
        => new(TriggerName, Arguments);
}
