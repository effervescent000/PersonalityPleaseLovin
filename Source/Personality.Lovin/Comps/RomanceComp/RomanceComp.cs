using System.Collections.Generic;
using Verse;

namespace Personality.Lovin;

public class RomanceComp : ThingComp
{
    public RomanceTracker RomanceTracker;
    public AttractionTracker AttractionTracker;
    public int LovinCooldownTicksRemaining;
    public List<LovinEvent> LovinJournal;

    public RomanceComp()
    {
    }

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        RomanceTracker ??= new();
        AttractionTracker ??= new(this);
        LovinJournal ??= new();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref RomanceTracker, "romance");
        Scribe_Deep.Look(ref AttractionTracker, "attraction", this);
        Scribe_Values.Look(ref LovinCooldownTicksRemaining, "lovinCD", 0);

        RomanceTracker ??= new();
        AttractionTracker ??= new(this);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            AttractionTracker.MakePreferences();
        }

        UpdateLovinJournal();
    }

    public void UpdateLovinJournal()
    {
        LovinTrackerComp lovinTrackerComp = Current.Game.GetComponent<LovinTrackerComp>();
        if (lovinTrackerComp != null && lovinTrackerComp.Ready)
        {
            LovinJournal = lovinTrackerComp.GetEventsFor((Pawn)parent);
        }
    }

    public void Notify_LovinTrackerReady(LovinTrackerComp comp)
    {
        LovinJournal = comp.GetEventsFor((Pawn)parent);
    }

    public override void CompTick()
    {
        RomanceTracker?.Tick();
        AttractionTracker.Tick();
        LovinCooldownTicksRemaining--;
    }
}