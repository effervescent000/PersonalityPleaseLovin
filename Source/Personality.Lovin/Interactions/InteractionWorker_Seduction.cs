using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class InteractionWorker_Seduction : InteractionWorker
{
    private readonly SimpleCurve resistSeductionByPurity = new()
    {
        new CurvePoint(-1f, 0.75f),
        new CurvePoint(1f, 1.25f),
    };

    private float SuccessChance(Pawn actor, Pawn target, out HistoryEventDef def)
    {
        def = null;
        // first, we should determine if the target actually wants to resist.
        if (RelationshipHelper.IsLoverOf(actor, target))
        {
            if (LovinHelper.DoesTargetAcceptIntimacy(actor, target))
            {
                //Find.HistoryEventsManager.RecordEvent(new HistoryEvent(LovinEventDefOf.PP_SeductionWilling, target.Named(HistoryEventArgsNames.Doer), actor.Named(HistoryEventArgsNames.Subject)));

                def = LovinEventDefOf.PP_SeductionWilling;

                return 1f;
            }
        }
        else
        {
            if (LovinHelper.DoesTargetAcceptHookup(actor, target))
            {
                //Find.HistoryEventsManager.RecordEvent(new HistoryEvent(LovinEventDefOf.PP_SeductionWilling, target.Named(HistoryEventArgsNames.Doer), actor.Named(HistoryEventArgsNames.Subject)));
                def = LovinEventDefOf.PP_SeductionWilling;
                return 1f;
            }
        }

        float roll = Rand.Value;

        MindComp mind = target.GetComp<MindComp>();

        if (RelationshipHelper.WouldBeCheating(target, actor))
        {
            // cheating multiplier should be present but have a reduced effect here
            float cheatingMod = (target.GetStatValue(LovinDefOf.PP_CheatingLikelihood) - .5f) * 0.25f + 0.5f;
            roll *= cheatingMod;
        }

        PersonalityNode purity = mind.GetNode(PersonalityDefOf.PP_Purity);
        roll *= resistSeductionByPurity.Evaluate(purity.FinalRating.Value);

        return roll;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        // this just sets the letter stuff to all null so we're leaving it in
        base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef, out lookTargets);

        // add seduction pheromones regardless of whether the interaction succeeds
        recipient.health.AddHediff(HediffMaker.MakeHediff(LovinDefOf.PP_SeductionPheromones, recipient));

        // this should be a pretty high chance to succeed
        if (SuccessChance(initiator, recipient, out HistoryEventDef eventDef) > 0.25f)
        {
            Building_Bed bed = LovinHelper.FindBed(initiator, recipient);
            if (bed == null)
            {
                Log.Warning("Unable to find a bed for seduction");
                return;
            }
            eventDef ??= LovinEventDefOf.PP_SeductionResistFailed;
            initiator.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoSeducedLovinLead, recipient, bed, bed.GetSleepingSlotPos(0)));
            recipient.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoSeducedLovin, initiator, bed, bed.GetSleepingSlotPos(1)));
            initiator.jobs.EndCurrentJob(JobCondition.InterruptForced);
            recipient.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
        else
        {
            eventDef ??= LovinEventDefOf.PP_SeductionResisted;
        }
        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(eventDef, recipient.Named(HistoryEventArgsNames.Doer), initiator.Named(HistoryEventArgsNames.Subject)));
    }

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient) => 0f;
}