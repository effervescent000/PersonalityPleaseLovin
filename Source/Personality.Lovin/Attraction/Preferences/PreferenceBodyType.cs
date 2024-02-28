using RimWorld;
using Verse;

namespace Personality.Lovin;

public class PreferenceBodyType : Preference, IExposable
{
    public BodyTypeDef Def;

    public override string Label => Def.defName;

    public PreferenceBodyType()
    {
    }

    public override float CalcAttractionEffect(Pawn pawn)
    {
        if (pawn.story?.bodyType.defName == Def.defName)
        {
            return Value;
        }
        return 0f;
    }

    public void ExposeData()
    {
        Scribe_Defs.Look(ref Def, "def");
        Scribe_Values.Look(ref Value, "value");
    }
}