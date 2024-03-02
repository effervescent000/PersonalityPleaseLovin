using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobGiver_SeekLovin : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn)
    {
        if (pawn.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin) == null)
        {
            return 0f;
        }
        float lovinChance = LovinHelper.GetChanceToSeekLovin(pawn);
        TimeAssignmentDef timeAssignmentDef = pawn.timetable == null ? TimeAssignmentDefOf.Anything : pawn.timetable.CurrentAssignment;
        if (timeAssignmentDef == TimeAssignmentDefOf.Sleep)
        {
            return lovinChance * 0.25f;
        }
        return lovinChance;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.IsAsexual())
        {
            return null;
        }
        if (pawn.ageTracker.AgeBiologicalYearsFloat < 16f)
        {
            return null;
        }

        // before we look for a partner, decide what kind of lovin' we're looking for. if we roll
        // above a .25, we're looking for a partner. if we're below that, the pawn just does self lovin'

        if (Rand.Value <= .25f)
        {
            return LovinHelper.TryDoSelfLovin(pawn);
        }

        return LovinHelper.TrySeekLovin(pawn);
    }
}