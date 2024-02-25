using Personality.Core;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobDriver_DoSeducedLovin : JobDriver
{
    private readonly TargetIndex partnerInd = TargetIndex.A;
    private readonly TargetIndex bedInd = TargetIndex.B;
    private readonly TargetIndex slotInd = TargetIndex.C;

    // TODO eventually would like to make this number a bit more dynamic
    private readonly int lovinDuration = 2000;

    private Building_Bed Bed => (Building_Bed)job.GetTarget(bedInd);
    private Pawn Actor => GetActor();
    private Pawn Partner => (Pawn)job.GetTarget(partnerInd);

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(bedInd);
        this.FailOnDespawnedOrNull(partnerInd);

        yield return Toils_Reserve.Reserve(slotInd, Bed.SleepingSlotsCount, 0);
        yield return Toils_Goto.Goto(slotInd, PathEndMode.OnCell);
        yield return new Toil
        {
            initAction = delegate { ticksLeftThisToil = 300; },
            tickAction = delegate
            {
                if (CoreLovinHelper.IsInOrByBed(Bed, Partner))
                {
                    ticksLeftThisToil = 0;
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        // get in the bed
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

        // do lovin'
        yield return new Toil
        {
            initAction = delegate
            {
                ticksLeftThisToil = lovinDuration;
            },
            tickAction = delegate
            {
                if (ticksLeftThisToil % 100 == 0)
                {
                    Actor.ThrowHeart();
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };

        yield return new Toil
        {
            initAction = delegate
            {
                LovinHelper.EvaluateLovin(new LovinProps(LovinContext.Seduced, Actor, Partner));
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}