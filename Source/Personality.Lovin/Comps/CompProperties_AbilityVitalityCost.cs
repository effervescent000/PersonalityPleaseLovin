using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class CompProperties_AbilityVitalityCost : CompProperties_AbilityEffect
{
    public float vitalityCost;

    public CompProperties_AbilityVitalityCost()
    {
        compClass = typeof(CompAbilityEffect_VitalityCost);
    }
}