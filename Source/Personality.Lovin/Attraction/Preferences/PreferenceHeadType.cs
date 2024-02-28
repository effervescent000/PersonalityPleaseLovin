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
        throw new NotImplementedException();
    }

    public override string Label => Def.defName;
}