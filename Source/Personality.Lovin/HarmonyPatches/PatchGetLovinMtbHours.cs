using HarmonyLib;
using RimWorld;

namespace Personality.Lovin.HarmonyPatches;

[HarmonyPatch(typeof(LovePartnerRelationUtility), nameof(LovePartnerRelationUtility.GetLovinMtbHours))]
public static class PatchGetLovinMtbHours
{
    public static void Postfix(ref float __result)
    {
        __result = -1f;
    }
}