using HarmonyLib;
using Personality.Lovin.HarmonyPatches;
using Verse;

namespace Personality.Lovin;

[StaticConstructorOnStartup]
internal static class OnStartup
{
    static OnStartup()
    {
        Harmony harmony = new("effervescent.personalityplease.lovin");
        harmony.PatchAll();
    }
}