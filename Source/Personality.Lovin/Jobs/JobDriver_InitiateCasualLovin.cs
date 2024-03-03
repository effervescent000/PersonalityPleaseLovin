using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobDriver_InitiateCasualLovin : JobDriver
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
        if (TargetPawn == null)
        {
            Log.Warning("TargetPawn is null in JobDriver");
        }
        if (Bed == null)
        {
            Log.Warning("Bed is null in JobDriver");
        }

        this.FailOnDespawnedNullOrForbidden(TargetPawnIndex);

        // 0
        Toil goToTarget = Toils_Interpersonal.GotoInteractablePosition(TargetPawnIndex);
        goToTarget.socialMode = RandomSocialMode.Off;
        goToTarget.AddFailCondition(() => !GeneralHelper.IsTargetInRange(Actor, TargetPawn));
        yield return goToTarget;

        // 1
        Toil wait = Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
        wait.socialMode = RandomSocialMode.Off;
        yield return wait;

        // 2
        Toil proposeCasualLovin = new()
        {
            defaultCompleteMode = ToilCompleteMode.Delay,
            initAction = delegate
            {
                ticksLeftThisToil = 50;
                FleckMaker.ThrowMetaIcon(Actor.Position, Actor.Map, FleckDefOf.Heart);
            }
        };
        proposeCasualLovin.AddFailCondition(() => !TargetPawn.IsOk());
        yield return proposeCasualLovin;

        // 3
        Toil awaitResponse = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                targetAccepted = LovinHelper.DoesTargetAcceptHookup(Actor, TargetPawn);
            }
        };
        awaitResponse.AddFailCondition(() => !DidTargetAccept);
        yield return awaitResponse;

        // target responds here
        Toil giveLovinJobsOrEnd = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                List<RulePackDef> resultList = new();
                if (!DidTargetAccept)
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.IncapIcon);
                    resultList.Add(LovinRulePackDefOf.PP_HookupFailed);
                    Find.PlayLog.Add(new PlayLogEntry_Interaction(LovinRulePackDefOf.PP_TriedHookup, pawn, TargetPawn, resultList));
                    RomanceComp comp = pawn.GetComp<RomanceComp>();
                    comp.RomanceTracker.RejectionList.Add(new RejectionItem(TargetPawn));
                    Actor.needs.mood.thoughts.memories.TryGainMemory(LovinThoughtDefOf.PP_TurnedMeDownForHookup, TargetPawn);
                    TargetPawn.needs.mood.thoughts.memories.TryGainMemory(LovinThoughtDefOf.PP_HadToRejectSomeoneForHookup, Actor);
                }
                else
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.Heart);
                    resultList.Add(LovinRulePackDefOf.PP_HookupSucceeded);
                    Find.PlayLog.Add(new PlayLogEntry_Interaction(LovinRulePackDefOf.PP_TriedHookup, pawn, TargetPawn, resultList));
                    Actor.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinDefOf.DoCasualLovin, TargetPawn, Bed, Bed.GetSleepingSlotPos(0)), JobTag.SatisfyingNeeds);
                    TargetPawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(LovinDefOf.DoCasualLovin, Actor, Bed, Bed.GetSleepingSlotPos(1)), JobTag.SatisfyingNeeds);
                    TargetPawn.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                    Actor.jobs.EndCurrentJob(JobCondition.InterruptOptional);
                }
            }
        };

        yield return giveLovinJobsOrEnd;
    }
}