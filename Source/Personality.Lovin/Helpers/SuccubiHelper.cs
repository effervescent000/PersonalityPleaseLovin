using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public static class SuccubiHelper
{
    public static SimpleCurve SuccubiAgingCurve = new()
    {
        new CurvePoint(1f, -2f),
        new CurvePoint(0.5f, 0f),
        new CurvePoint(0f, 12f),
    };

    public static void OffsetVitality(Pawn pawn, float amount)
    {
        Gene_Resource vitality = (Gene_Resource)pawn.genes.GetGene(LovinDefOf.PP_VitalityRoot);
        vitality.Value += amount;
    }

    public static bool IsLoveFeeder(this Pawn pawn) => pawn.genes.HasGene(LovinDefOf.PP_LoveFeeder);

    public static float GetRapidAgingMultiplier(Pawn pawn)
    {
        var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(LovinDefOf.PP_VitalityLost);
        if (hediff == null)
        {
            return 1f;
        }

        var comp = hediff.TryGetComp<HediffComp_RapidAging>();
        var maxMultiplier = comp.Props.multiplierAtMax;
        SimpleCurve agingCurve = new()
        {
            new CurvePoint(0f, 1f),
            new CurvePoint(0.6f, maxMultiplier * 0.1f),
            new CurvePoint(0.9f, maxMultiplier * 0.25f),
            new CurvePoint(1f, maxMultiplier)
        };
        return agingCurve.Evaluate(hediff.Severity);
    }
}