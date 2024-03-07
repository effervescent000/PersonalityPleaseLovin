using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobGiver_SeekLovin : ThinkNode_JobGiver

{
    public override float GetPriority(Pawn pawn)
    {
        if (pawn.IsAsexual()) return 0f;
        if (pawn.ageTracker.AgeBiologicalYears < 16) return 0f;

        RomanceComp comp = pawn.GetComp<RomanceComp>();
        if (comp?.LovinCooldownTicksRemaining > 0) return 0f;

        var lovinNeed = (Need_Lovin)pawn.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);

        if (lovinNeed == null)
        {
            return 0f;
        }

        SimpleCurve LovinNeedCurve = new()
        {
            new CurvePoint(1f, 0f),
            new CurvePoint(lovinNeed.Horny, 4f),
            new CurvePoint(lovinNeed.Desperate, 8f),
            new CurvePoint(0f, 12f),
        };

        float lovinChance = LovinNeedCurve.Evaluate(lovinNeed.CurLevel);

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

        MindComp mind = pawn.GetComp<MindComp>();

        JobDef job = LovinJobDefOf.PP_InitiateIntimateLovin;
        Pawn partner = LovinHelper.FindPartnerForIntimacy(pawn, mind);

        bool isCheating = false;
        List<Pawn> existingPartners = new();

        // if partner is null, then obviously we're looking for a hookup. otherwise, calculate the
        // roll for a hookup
        if (partner == null)
        {
            partner = LovinHelper.FindPartnerForHookup(pawn, mind);
            job = LovinJobDefOf.LeadHookup;
        }
        else
        {
            float hookupThreshold = 0.5f;

            //if a pawn is monogamous and partnered, calculate effect of fidelity
            if (mind.GetQuirkByDef(LovinQuirkDefOf.PP_Monogamous, out Quirk _) && pawn.IsPartnered(out existingPartners))
            {
                hookupThreshold -= 0.2f;
                hookupThreshold *= pawn.GetStatValue(LovinDefOf.PP_CheatingLikelihood);

                // lastly add cheating multiplier from settings
                hookupThreshold *= LovinMod.Settings.CheatingModifier.Value / 100f;
            }

            //if (mind.GetQuirkByDef(LovinQuirkDefOf.PP_RomanceSeeking, out Quirk romanceDesire))
            //{
            //    hookupThreshold *= LovinHelper.chanceToHookupByRomanceDesire.Evaluate(romanceDesire.Value);
            //}

            if (Rand.Value < hookupThreshold)
            {
                partner = LovinHelper.FindPartnerForHookup(pawn, mind);
                job = LovinJobDefOf.LeadHookup;
                isCheating = RelationshipHelper.WouldBeCheating(pawn, partner);
            }
        }

        // if we can't actually find a partner, then the pawn either gives up and does something
        // else or does self lovin'
        if (partner == null)
        {
            if (Rand.Value < .75f)
            {
                return null;
            }
            else
            {
                return LovinHelper.TryDoSelfLovin(pawn);
            }
        }

        Building_Bed bed = LovinHelper.FindBed(pawn, partner);
        if (bed == null)
        {
            return null;
        }

        LovinHelper.MakeLovinMessage(pawn, partner, existingPartners, isCheating, job);
        LovinHelper.ResetLovinCooldown(pawn);
        return JobMaker.MakeJob(job, partner, bed);
    }
}