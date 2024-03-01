using Verse;

namespace Personality.Lovin;

public class Settings : ModSettings
{
    public SettingValues<int> CheatingModifier = new(100, "PP.CheatingModifier.Label", "PP.CheatingModifier.Desc", 0, 300);

    public override void ExposeData()
    {
        base.ExposeData();
    }
}