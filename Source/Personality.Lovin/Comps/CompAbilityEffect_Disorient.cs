using RimWorld;
using Verse;

namespace Personality.Lovin;

public class CompAbilityEffect_Disorient : CompAbilityEffect
{
    private new CompProperties_AbilityDisorient Props => (CompProperties_AbilityDisorient)props;

    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        GenExplosion.DoExplosion(target.Cell, parent.pawn.MapHeld, Props.radius, LovinDefOf.PP_DisorientDamage, parent.pawn, -1);
    }
}