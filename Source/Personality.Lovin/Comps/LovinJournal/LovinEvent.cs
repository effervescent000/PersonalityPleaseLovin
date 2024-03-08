using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public struct LovinEvent : IExposable
{
    public Pawn initiator;
    public Pawn partner;
    public LovinContext context;
    public int tickCompleted;
    public int durationTicks;
    public float initiatorQuality;
    public float partnerQuality;
    public bool cheating;
    public bool married;

    public LovinEvent(LovinProps props, float initiatorQuality, float partnerQuality, bool isCheating)
    {
        initiator = props.Actor;
        partner = props.Partner;
        context = props.Context;
        durationTicks = props.Duration;
        tickCompleted = Find.TickManager.TicksGame;
        this.initiatorQuality = initiatorQuality;
        this.partnerQuality = partnerQuality;
        cheating = isCheating;

        // default for now
        married = false;
    }

    public void ExposeData()
    {
        Scribe_References.Look(ref initiator, "initiator");
        Scribe_References.Look(ref partner, "partner");
        Scribe_Values.Look(ref context, "context");
        Scribe_Values.Look(ref tickCompleted, "tickCompleted");
        Scribe_Values.Look(ref durationTicks, "duration");
        Scribe_Values.Look(ref initiatorQuality, "initQuality");
        Scribe_Values.Look(ref partnerQuality, "partnerQuality");
    }
}