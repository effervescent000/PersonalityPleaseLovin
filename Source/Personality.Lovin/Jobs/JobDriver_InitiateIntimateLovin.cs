using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobDriver_InitiateIntimateLovin : JobDriver
{
    public bool targetAccepted = true;
    public bool DidTargetAccept => targetAccepted;

    private Pawn Actor => GetActor();
    private Pawn TargetPawn => (Pawn)TargetThingA;
    private Building_Bed Bed => (Building_Bed)TargetThingB;
    private TargetIndex TargetPawnIndex => TargetIndex.A;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetPawnIndex);

        Toil goToTarget = Toils_Interpersonal.GotoInteractablePosition(TargetPawnIndex);
        goToTarget.socialMode = RandomSocialMode.Off;
        goToTarget.AddFailCondition(() => !GeneralHelper.IsTargetInRange(Actor, TargetPawn));
        yield return goToTarget;

        Toil wait = Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
        wait.socialMode = RandomSocialMode.Off;
        yield return wait;

        Toil proposeCasualLovin = new()
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            initAction = delegate
            {
                ticksLeftThisToil = 50;
                Actor.ThrowHeart();
            }
        };
        proposeCasualLovin.AddFailCondition(() => !TargetPawn.IsOk());
        yield return proposeCasualLovin;

        Toil awaitResponse = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                targetAccepted = LovinHelper.DoesTargetAcceptIntimacy(Actor, TargetPawn);
            }
        };
        awaitResponse.AddFailCondition(() => !DidTargetAccept);
        yield return awaitResponse;

        Toil giveLovinJobsOrEnd = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                List<RulePackDef> resultList = new();
                if (!DidTargetAccept)
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.IncapIcon);
                    resultList.Add(LovinRulePackDefOf.PP_IntimacyFailed);
                    Find.PlayLog.Add(new PlayLogEntry_Interaction(LovinRulePackDefOf.PP_TriedIntimacy, pawn, TargetPawn, resultList));
                    RomanceComp comp = pawn.GetComp<RomanceComp>();
                    comp.RomanceTracker.RejectionList.Add(new RejectionItem(TargetPawn));
                    Actor.needs.mood.thoughts.memories.TryGainMemory(LovinThoughtDefOf.PP_TurnedMeDownForIntimacy, TargetPawn);
                    TargetPawn.needs.mood.thoughts.memories.TryGainMemory(LovinThoughtDefOf.PP_HadToRejectSomeoneForIntimacy, Actor);
                }
                else
                {
                    TargetPawn.ThrowHeart();
                    resultList.Add(LovinRulePackDefOf.PP_IntimacySucceeded);
                    Find.PlayLog.Add(new PlayLogEntry_Interaction(LovinRulePackDefOf.PP_TriedIntimacy, pawn, TargetPawn, resultList));
                    Actor.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoIntimateLovinLead, TargetPawn, Bed, Bed.GetSleepingSlotPos(0)), JobTag.SatisfyingNeeds);
                    TargetPawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinJobDefOf.PP_DoIntimateLovin, Actor, Bed, Bed.GetSleepingSlotPos(1)), JobTag.SatisfyingNeeds);
                    TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                    Actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                }
            }
        };

        yield return giveLovinJobsOrEnd;
    }
}