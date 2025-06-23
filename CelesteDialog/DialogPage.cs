namespace CelesteDialog;

/// <summary>
/// A page in <see cref="DialogPlotEntry"/>, a kind of <see cref="DialogEntry"/>.
/// </summary>
public abstract class DialogPage : ICloneable
{
    public abstract DialogPage Clone();

    object ICloneable.Clone()
        => Clone();
}
