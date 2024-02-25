using RimWorld;
using Verse;

namespace Personality.Lovin;

public class CompAbilityEffect_VitalityCost : CompAbilityEffect
{
    public new CompProperties_AbilityVitalityCost Props => (CompProperties_AbilityVitalityCost)props;

    private bool HasEnoughVitality
    {
        get
        {
            Gene_Resource vitalityGene = parent.pawn.genes?.GetFirstGeneOfType<Gene_Resource>();
            if (vitalityGene == null || vitalityGene.Value < Props.vitalityCost)
            {
                return false;
            }
            return true;
        }
    }

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        SuccubiHelper.OffsetVitality(parent.pawn, Props.vitalityCost * -1f);
    }

    public override bool GizmoDisabled(out string reason)
    {
        if (!HasEnoughVitality)
        {
            reason = "PP_AbilityDisabledNoVitality".Translate();
            return true;
        }
        reason = null;
        return false;
    }
}