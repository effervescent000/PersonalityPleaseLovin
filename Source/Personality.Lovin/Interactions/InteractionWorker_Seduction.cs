using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class InteractionWorker_Seduction : InteractionWorker
{
    private float SuccessChance(Pawn actor, Pawn target)
    {
        // TODO MATH, probably based on personality factors.

        // for now let's just make it 100% for testing.
        return 1f;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        // this just sets the letter stuff to all null so we're leaving it in
        base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef, out lookTargets);

        // add seduction pheromones regardless of whether the interaction succeeds
        recipient.health.AddHediff(HediffMaker.MakeHediff(LovinDefOf.PP_SeductionPheromones, recipient));

        // this should be a pretty high chance to succeed
        if (SuccessChance(initiator, recipient) > 0.25f)
        {
            Building_Bed bed = LovinHelper.FindBed(initiator, recipient);
            if (bed == null)
            {
                Log.Warning("Unable to find a bed for seduction");
                return;
            }
            initiator.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoSeducedLovinLead, recipient, bed, bed.GetSleepingSlotPos(0)));
            recipient.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoSeducedLovin, initiator, bed, bed.GetSleepingSlotPos(1)));
            initiator.jobs.EndCurrentJob(JobCondition.InterruptForced);
            recipient.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient) => 0f;
}