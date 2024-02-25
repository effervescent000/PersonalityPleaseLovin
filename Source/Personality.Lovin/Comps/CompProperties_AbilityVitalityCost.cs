using RimWorld;

namespace Personality.Lovin;

public class CompProperties_AbilityVitalityCost : CompProperties_AbilityEffect
{
    public float vitalityCost;

    public CompProperties_AbilityVitalityCost()
    {
        compClass = typeof(CompAbilityEffect_VitalityCost);
    }
}