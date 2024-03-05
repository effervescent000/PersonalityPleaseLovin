using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class LovinTrackerComp : GameComponent
{
    private List<LovinEvent> lovinEvents;

    public LovinTrackerComp(Game _)
    {
    }

    public override void FinalizeInit()
    {
        lovinEvents ??= new();
    }

    public void AddEvent(LovinProps props, float initQuality, float partnerQuality)
    {
        lovinEvents.Add(new(props, initQuality, partnerQuality));
    }

    public List<LovinEvent> GetEventsFor(Pawn pawn)
    {
        var query = (from e in lovinEvents
                     where e.initiator.Equals(pawn) || e.partner.Equals(pawn)
                     select e).ToList();
        return query;
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref lovinEvents, "events", LookMode.Deep);

        lovinEvents ??= new();
    }
}