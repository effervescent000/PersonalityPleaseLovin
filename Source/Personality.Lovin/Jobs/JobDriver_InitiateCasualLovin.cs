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
                FleckMaker.ThrowMetaIcon(Actor.Position, Actor.Map, FleckDefOf.Heart);
            }
        };
        proposeCasualLovin.AddFailCondition(() => !TargetPawn.IsOk());
        yield return proposeCasualLovin;

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

        Toil giveLovinJobsOrEnd = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                if (!DidTargetAccept)
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.IncapIcon);
                    RomanceComp comp = pawn.GetComp<RomanceComp>();
                    comp.RomanceTracker.RejectionList.Add(new RejectionItem(TargetPawn));
                    Actor.needs.mood.thoughts.memories.TryGainMemory(LovinDefOf.PP_TurnedMeDownForHookup, TargetPawn);
                    TargetPawn.needs.mood.thoughts.memories.TryGainMemory(LovinDefOf.PP_HadToRejectSomeoneForHookup, Actor);
                }
                else
                {
                    FleckMaker.ThrowMetaIcon(TargetPawn.Position, TargetPawn.Map, FleckDefOf.Heart);
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