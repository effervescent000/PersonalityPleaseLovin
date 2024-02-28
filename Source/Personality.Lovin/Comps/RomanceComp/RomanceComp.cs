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
        RomanceTracker = new RomanceTracker();
        AttractionTracker = new(parent as Pawn);
    }

    public override void PostExposeData()
    {
        Scribe_Deep.Look(ref RomanceTracker, "romance");
        Scribe_Deep.Look(ref AttractionTracker, "attraction", new object[] { parent as Pawn });
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            AttractionTracker.Initialize();
        }
    }

    public override void CompTick()
    {
        RomanceTracker?.Tick();
        AttractionTracker.Tick();
    }
}