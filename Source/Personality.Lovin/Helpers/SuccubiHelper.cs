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
    public static void OffsetVitality(Pawn pawn, float amount)
    {
        Gene_Resource vitality = (Gene_Resource)pawn.genes.GetGene(LovinDefOf.PP_VitalityRoot);
        vitality.Value += amount;
    }

    public static bool IsLoveFeeder(this Pawn pawn) => pawn.genes.HasGene(LovinDefOf.PP_LoveFeeder);
}