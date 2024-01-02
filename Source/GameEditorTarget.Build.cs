using Flax.Build;

/// <summary>
/// 
/// </summary>
public class GameEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add("Gravity");
        Modules.Add("Game");
    }
}
