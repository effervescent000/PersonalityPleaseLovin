using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Personality.Core;

namespace Personality.Lovin;

public class JobDriver_DoSelfLovin : JobDriver
{
    private readonly TargetIndex bedInd = TargetIndex.A;
    private readonly TargetIndex slotInd = TargetIndex.B;

    private readonly int ticks = 500;
    private readonly int ticksBetweenHearts = 100;

    private Building_Bed Bed => (Building_Bed)job.GetTarget(bedInd);
    private Pawn Actor => GetActor();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Bed, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(bedInd);

        yield return Toils_Reserve.Reserve(bedInd, 1, 0);
        yield return Toils_Goto.Goto(slotInd, PathEndMode.OnCell);

        // get in bed
        Toil layDown = new();
        layDown.initAction = delegate
        {
            layDown.actor.pather.StopDead();
            JobDriver curDriver = layDown.actor.jobs.curDriver;
            curDriver.asleep = false;
            layDown.actor.jobs.posture = PawnPosture.LayingInBed;
        };
        layDown.tickAction = delegate
        {
            Actor.GainComfortFromCellIfPossible();
        };
        yield return layDown;

        // self love
        yield return new Toil
        {
            initAction = delegate
            {
                ticksLeftThisToil = ticks;
            },
            tickAction = delegate
            {
                if (ticksLeftThisToil % ticksBetweenHearts == 0)
                {
                    Actor.ThrowHeart();
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };

        // gain lovin' need
        yield return new Toil
        {
            initAction = delegate
            {
                //Actor.IncreaseLovinNeed(1f);
                LovinHelper.EvaluateLovin(new LovinProps(LovinContext.SelfLovin, Actor));
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}