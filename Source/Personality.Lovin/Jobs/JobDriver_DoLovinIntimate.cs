using RimWorld;

namespace Personality.Lovin;

public class JobDriver_DoLovinIntimate : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GenDate.TicksPerHour * 1;
        ticksForEnhancer = GenDate.TicksPerHour * 2;
        context = LovinContext.Intimate;
    }
}