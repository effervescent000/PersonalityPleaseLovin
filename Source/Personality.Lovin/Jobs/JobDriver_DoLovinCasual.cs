namespace Personality.Lovin;

public class JobDriver_DoLovinCasual : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GeneralHelper.GetHourBasedDuration(0.5f);
        ticksForEnhancer = GeneralHelper.GetHourBasedDuration(1f);
        context = LovinContext.Casual;
    }
}