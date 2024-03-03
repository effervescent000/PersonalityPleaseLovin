using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public class JobDriver_DoIntimateLovin : JobDriver
{
    private readonly TargetIndex PartnerInd = TargetIndex.A;
    private readonly TargetIndex BedInd = TargetIndex.B;
    private readonly TargetIndex SlotInd = TargetIndex.C;
    private const int TicksBetweenHeartMotes = 100;
    private readonly int ticksBase = GeneralHelper.GetHourBasedDuration(1f);
    private readonly int ticksForEnhancer = GeneralHelper.GetHourBasedDuration(2f);

    private Building_Bed Bed => (Building_Bed)job.GetTarget(BedInd);
    private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);
    private Pawn Actor => GetActor();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(BedInd);
        this.FailOnDespawnedOrNull(PartnerInd);

        yield return Toils_Reserve.Reserve(BedInd, 2, 0);
        yield return Toils_Goto.Goto(SlotInd, PathEndMode.OnCell);
        // wait for both pawns to be near the bed
        Toil wait = new()
        {
            initAction = delegate { ticksLeftThisToil = 500; },
            tickAction = delegate
            {
                if (LovinHelper.IsInOrByBed(Bed, Partner))
                {
                    ticksLeftThisToil = 0;
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };

        yield return wait;
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
                ticksLeftThisToil = ticksBase;
            },
            tickAction = delegate
            {
                if (ticksLeftThisToil % TicksBetweenHeartMotes == 0)
                {
                    Actor.ThrowHeart();
                }
            },
            defaultCompleteMode = ToilCompleteMode.Delay
        };

        yield return LovinHelper.FinishLovin(new LovinProps(LovinContext.Intimate, Actor, Partner));
    }
}