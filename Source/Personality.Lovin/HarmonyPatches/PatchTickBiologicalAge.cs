using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace Personality.Lovin.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_AgeTracker), "TickBiologicalAge")]
public static class PatchTickBiologicalAge
{
    public static bool Prefix(int interval, Pawn_AgeTracker __instance, ref float ___progressToNextBiologicalTick, ref long ___ageBiologicalTicksInt, ref Pawn ___pawn)
    {
        if (!___pawn.genes?.HasGene(LovinDefOf.PP_VitalityRoot) ?? false)
        {
            return true;
        }

        float ticksThisCall = __instance.BiologicalTicksPerTick * (float)interval;
        if (ticksThisCall > 0f) return true;

        int num = Mathf.FloorToInt(ticksThisCall);

        ___ageBiologicalTicksInt += num;

        return false;
    }
}