using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class DamageWorker_Disorient : DamageWorker
{
    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        if (victim.def.defName == "Human" && victim != dinfo.Instigator)
        {
            CompAbilityEffect_GiveMentalState.TryGiveMentalStateWithDuration(MentalStateDefOf.Wander_Psychotic, (Pawn)victim, LovinAbilityDefOf.PP_DisorientAbility, null, (Pawn)dinfo.Instigator);
        }

        return new DamageResult();
    }
}