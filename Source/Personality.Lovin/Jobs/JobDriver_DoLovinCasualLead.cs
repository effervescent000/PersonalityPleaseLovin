namespace Personality.Lovin;

public class JobDriver_DoLovinCasualLead : JobDriver_DoLovinCasual
{
    protected override void SetInitiator()
    {
        isInitiator = true;
    }
}