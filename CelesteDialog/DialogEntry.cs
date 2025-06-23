namespace CelesteDialog;

/// <summary>
/// An entry in <see cref="DialogDocument"/>, maybe
/// be a <see cref="DialogPlotEntry"/> or just a <see cref="DialogTranslationEntry"/>.
/// </summary>
public abstract class DialogEntry : ICloneable
{
    public abstract DialogEntry Clone();

    object ICloneable.Clone() => Clone();
}
