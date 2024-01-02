using Flax.Build;

/// <summary>
/// 
/// </summary>
public class GameTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("Game");
    }
}
