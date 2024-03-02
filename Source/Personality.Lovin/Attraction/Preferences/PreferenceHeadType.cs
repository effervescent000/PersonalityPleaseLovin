using System;
using Verse;

namespace Personality.Lovin;

public class PreferenceHeadType : Preference, IExposable
{
    public HeadTypeDef Def;

    public override float CalcAttractionEffect(Pawn pawn)
    {
        if (pawn.story.headType.defName == Def.defName)
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

    public override string Label => Def.defName;
}