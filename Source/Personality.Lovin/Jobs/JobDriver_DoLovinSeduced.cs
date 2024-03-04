namespace Personality.Lovin;

public class JobDriver_DoLovinSeduced : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = GeneralHelper.GetHourBasedDuration(2f);
        ticksForEnhancer = GeneralHelper.GetHourBasedDuration(3f);
        context = LovinContext.Seduced;
        TicksBetweenHeartMotes = 75;
    }
}