using System.Diagnostics;

namespace CelesteDialog;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class DialogPortraitState : ICloneable, IEquatable<DialogPortraitState?>
{
    public string Character { get; set; }
    public string SubCharacter { get; set; }
    public bool AtRight { get; set; }
    public bool AnchorBottom { get; set; }
    public bool Flip { get; set; }
    public bool UpsideDown { get; set; }
    public bool Pop { get; set; }

    public DialogPortraitState()
        : this(string.Empty, string.Empty, false)
    {
    }

    public DialogPortraitState(string character, string subCharacter, bool atRight)
        : this(character, subCharacter, atRight, false, false, false, false)
    {
    }

    public DialogPortraitState(
        string character, string subCharacter, bool atRight,
        bool anchorBottom = false, bool flip = false,
        bool upsideDown = false, bool pop = false
        )
    {
        Character = character;
        SubCharacter = subCharacter;
        AtRight = atRight;
        UpsideDown = upsideDown;
        Pop = pop;
        Flip = flip;
        AnchorBottom = anchorBottom;
    }

    object ICloneable.Clone()
        => Clone();

    public DialogPortraitState Clone()
        => new(Character, SubCharacter, AtRight, AnchorBottom, Flip, UpsideDown, Pop);

    public void Reset()
        => (Character, SubCharacter, AtRight, AnchorBottom, Flip, UpsideDown, Pop) =
        (string.Empty, string.Empty, false, false, false, false, false);

    public override bool Equals(object? obj) 
        => Equals(obj as DialogPortraitState);

    public bool Equals(DialogPortraitState? other)
        => other is not null &&
            Character.Equals(other.Character, StringComparison.OrdinalIgnoreCase) &&
            SubCharacter.Equals(other.SubCharacter, StringComparison.OrdinalIgnoreCase) &&
            AtRight == other.AtRight &&
            AnchorBottom == other.AnchorBottom &&
            Flip == other.Flip &&
            UpsideDown == other.UpsideDown &&
            Pop == other.Pop;

    public override int GetHashCode()
        => HashCode.Combine(
            Character.GetHashCode(StringComparison.OrdinalIgnoreCase),
            SubCharacter.GetHashCode(StringComparison.OrdinalIgnoreCase),
            AtRight, AnchorBottom, Flip, UpsideDown, Pop
            );

    private string DebuggerDisplay
    {
        get
        {
            string str = string.Empty;
            if (AnchorBottom)
                str += "(anchor bottom) ";
            str += $"[{Character} {(AtRight ? "right" : "left")} {SubCharacter}";
            if (UpsideDown) str += " upsidedown";
            if (Flip) str += " flip";
            if (Pop) str += " pop";
            return str + "]";
        }
    }

    public static bool operator ==(DialogPortraitState? left, DialogPortraitState? right)
        => EqualityComparer<DialogPortraitState>.Default.Equals(left, right);

    public static bool operator !=(DialogPortraitState? left, DialogPortraitState? right)
        => !(left == right);
}
