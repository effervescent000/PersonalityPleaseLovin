using RimWorld;

namespace Personality.Lovin;

public class JobDriver_DoLovinSeduced : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GenDate.TicksPerHour * 2;
        ticksForEnhancer = GenDate.TicksPerHour * 3;
        context = LovinContext.Seduced;
        TicksBetweenHeartMotes = 75;
    }
}