using Verse;

namespace Personality.Lovin;

internal class LovinMod : Mod
{
    public static Settings Settings;

    public LovinMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
    }
}