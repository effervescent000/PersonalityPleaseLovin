using RimWorld;

namespace Personality.Lovin;

public class SexualityValues
{
    public TraitDef TraitDef;
    public float chance;

    public SexualityValues(TraitDef traitDef, float chance)
    {
        TraitDef = traitDef;
        this.chance = chance;
    }
}