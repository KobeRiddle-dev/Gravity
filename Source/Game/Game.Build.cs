﻿using Flax.Build;
using Flax.Build.NativeCpp;

/// <summary>
/// 
/// </summary>
public class Game : GameModule
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // C#-only scripting
        BuildNativeCode = false;
    }

    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);
        options.PublicDependencies.Add("HideNSeekEditor");
        options.PublicDependencies.Add("HideNSeek");
        options.PublicDependencies.Add("HideNSeekEditor");
        options.PublicDependencies.Add("HideNSeek");
        options.PublicDependencies.Add("YAPCEditor");
        options.PublicDependencies.Add("YAPC");

        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;

        // Here you can modify the build options for your game module
        // To reference another module use: options.PublicDependencies.Add("Audio");
        // To add C++ define use: options.PublicDefinitions.Add("COMPILE_WITH_FLAX");
        // To learn more see scripting documentation.
    }
}
