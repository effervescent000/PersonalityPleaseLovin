using RimWorld;

namespace Personality.Lovin;

public class CompProperties_AbilityDisorient : CompProperties_AbilityEffect
{
    public float radius = 4f;

    public CompProperties_AbilityDisorient()
    {
        compClass = typeof(CompAbilityEffect_Disorient);
    }
}