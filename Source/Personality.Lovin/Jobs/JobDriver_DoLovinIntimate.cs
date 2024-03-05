using RimWorld;

namespace Personality.Lovin;

public class JobDriver_DoLovinIntimate : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GenDate.TicksPerHour * 1;
        ticksForEnhancer = GenDate.TicksPerHour * 2;
        context = LovinContext.Intimate;

        // duration should eventually actually check for a love enhancer but rn just use base ticks
        duration = ticksBase;
    }
}