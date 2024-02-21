using HarmonyLib;
using Personality.Lovin.HarmonyPatches;
using Verse;

namespace Personality.Lovin;

[StaticConstructorOnStartup]
internal static class OnStartup
{
    static OnStartup()
    {
        if (ModsConfig.IsActive("effervescent.personalityplease.romance"))
        {
            Settings.RomanceModuleActive = true;
        }

        Harmony harmony = new("effervescent.personalityplease.lovin");
        harmony.PatchAll();

        // now patch conditional modules
        if (Settings.RomanceModuleActive)
        {
            PPR.Patch(harmony);
        }
    }
}