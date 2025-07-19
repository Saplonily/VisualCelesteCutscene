namespace VisualCelesteCutscene;

// TODO undoable
public abstract class CommandAction
{
    public abstract void Execute();

    public abstract void Undo();
}
