using Verse;

namespace Personality.Lovin;

public class PreferenceHairStyle : Preference, IExposable
{
    public string Style;

    public override float CalcAttractionEffect(Pawn pawn)
    {
        if ((bool)(pawn.story?.hairDef.styleTags.Contains(Style)))
        {
            return Value;
        }
        return 0f;
    }

    public override string Label => Style;

    public void ExposeData()
    {
        Scribe_Values.Look(ref Style, "style");
        Scribe_Values.Look(ref Value, "value");
    }
}