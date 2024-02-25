using HarmonyLib;
using RimWorld;
using Verse;

namespace Personality.Lovin.HarmonyPatches;

[HarmonyPatch(typeof(Pawn_AgeTracker), nameof(Pawn_AgeTracker.BiologicalTicksPerTick), MethodType.Getter)]
public static class PatchBiologicalTicksPerTick
{
    public static void Postfix(ref float __result, Pawn ___pawn)
    {
        // ignore if pawn is underage
        if (___pawn.ageTracker.AgeBiologicalYears < 18f)
        {
            return;
        }

        // first check if has a vitality meter
        if (___pawn.genes.HasGene(LovinDefOf.PP_VitalityRoot))
        {
            Gene_Resource vitality = (Gene_Resource)___pawn.genes.GetGene(LovinDefOf.PP_VitalityRoot);
            __result *= SuccubiHelper.SuccubiAgingCurve.Evaluate(vitality.Value);
            return;
        }

        // if not succubus, then check for drained hediff
        if (___pawn.health.hediffSet.HasHediff(LovinDefOf.PP_VitalityLost))
        {
            __result *= SuccubiHelper.GetRapidAgingMultiplier(___pawn);
        }
    }
}