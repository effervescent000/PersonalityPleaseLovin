using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using static HarmonyLib.Code;

namespace Personality.Lovin;

public class JobDriver_DoLovin : JobDriver
{
    protected readonly TargetIndex PartnerInd = TargetIndex.A;
    protected readonly TargetIndex BedInd = TargetIndex.B;
    protected readonly TargetIndex SlotInd = TargetIndex.C;
    protected int TicksBetweenHeartMotes = 100;
    protected int ticksBase;
    protected int ticksForEnhancer;
    protected bool isInitiator = false;
    protected LovinContext context;

    protected Building_Bed Bed => (Building_Bed)job.GetTarget(BedInd);

    protected Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);
    protected Pawn Actor => GetActor();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
    }

    protected virtual void SetInitiator()
    {
        isInitiator = false;
    }

    protected virtual void JobSpecificSetup()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        SetInitiator();
        JobSpecificSetup();

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

        yield return new Toil
        {
            initAction = delegate
            {
                LovinHelper.EvaluateLovin(new LovinProps(context, Actor, Partner, isInitiator));
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}