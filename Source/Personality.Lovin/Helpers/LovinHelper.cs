using RimWorld;
using Personality.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Personality.Lovin;
using Verse.AI;

namespace Personality.Lovin;

public static class LovinHelper
{
    public static Job TryDoSelfLovin(Pawn pawn)
    {
        Building_Bed bed = CoreLovinHelper.FindBed(pawn);
        if (bed == null)
        {
            return null;
        }
        return JobMaker.MakeJob(LovinDefOf.DoSelfLovin, bed, bed.GetSleepingSlotPos(0));
    }

    public static void IncreaseLovinNeed(this Pawn pawn, float amount)
    {
        Need_Lovin need = pawn?.needs?.TryGetNeed<Need_Lovin>();
        if (need == null) return;

        need.CurLevel += amount;
    }

    public static void GetSatisfaction(LovinProps lovin)
    {
    }

    //public static float GetLovinQuality(Pawn primary, Pawn partner, LovinContext context)
    //{
    //    float quality = 0f;

    // // get partner's skill--eventually this will be a lovin' quality stat SkillRecord
    // partnerSkill = partner.skills.GetSkill(LovinDefOf.LovinSkill);

    // // get own pawn's lovin skill -- same as above, and this is intended to play a lesser role
    // SkillRecord primarySkill = primary.skills.GetSkill(LovinDefOf.LovinSkill);

    // // TODO -- in an intimate context, look at the pawns' relationship -- higher boosts lovin'

    //    // TODO -- in a casual context, look at each pawns' attraction to the other
    //}
}