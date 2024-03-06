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

        // duration should eventually actually check for a love enhancer but rn just use base ticks
        duration = ticksBase;
    }
}