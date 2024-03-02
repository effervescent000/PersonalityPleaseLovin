using Verse;

namespace Personality.Lovin;

public class RomanceComp : ThingComp
{
    public RomanceTracker RomanceTracker;
    public AttractionTracker AttractionTracker;

    public RomanceComp()
    {
    }

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        RomanceTracker ??= new();
        AttractionTracker ??= new(this);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref RomanceTracker, "romance");
        Scribe_Deep.Look(ref AttractionTracker, "attraction", this);

        RomanceTracker ??= new();
        AttractionTracker ??= new(this);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            AttractionTracker.MakePreferences();
        }
    }

    public override void CompTick()
    {
        RomanceTracker?.Tick();
        AttractionTracker.Tick();
    }
}