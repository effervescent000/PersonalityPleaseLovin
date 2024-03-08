using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Personality.Lovin;

public class LovinTrackerComp : GameComponent
{
    private List<LovinEvent> lovinEvents;
    public bool Ready = false;

    public LovinTrackerComp(Game _)
    {
    }

    public override void FinalizeInit()
    {
        lovinEvents ??= new();
        Ready = true;
    }

    public override void StartedNewGame()
    {
        base.StartedNewGame();
        SendNotify_Ready();
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        SendNotify_Ready();
    }

    public void SendNotify_Ready()
    {
        List<Pawn> spawnedPawns = (from pawn in Current.Game.CurrentMap.mapPawns.AllPawnsSpawned
                                   where pawn.def.defName == "Human" && pawn.ageTracker.AgeBiologicalYears >= 16
                                   select pawn).ToList();

        if (spawnedPawns != null)
        {
            foreach (Pawn pawn in spawnedPawns)
            {
                RomanceComp comp = pawn.GetComp<RomanceComp>();
                comp?.Notify_LovinTrackerReady(this);
            }
        }
    }

    public void AddEvent(LovinProps props, float initQuality, float partnerQuality, bool isCheating)
    {
        lovinEvents.Add(new(props, initQuality, partnerQuality, isCheating));
    }

    public List<LovinEvent> GetEventsFor(Pawn pawn)
    {
        List<LovinEvent> query = (from e in lovinEvents
                                  where e.initiator.ThingID == pawn.ThingID || e.partner.ThingID == pawn.ThingID
                                  orderby e.tickCompleted descending
                                  select e).ToList();

        return query;
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref lovinEvents, "events", LookMode.Deep);

        lovinEvents ??= new();
        Ready = true;
    }
}