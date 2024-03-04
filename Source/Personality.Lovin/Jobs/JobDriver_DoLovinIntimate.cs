namespace Personality.Lovin;

public class JobDriver_DoLovinIntimate : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GeneralHelper.GetHourBasedDuration(1f);
        ticksForEnhancer = GeneralHelper.GetHourBasedDuration(2f);
        context = LovinContext.Intimate;
    }
}