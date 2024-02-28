using UnityEngine;
using Verse;

namespace Personality.Lovin;

public class PreferenceHairColor : Preference, IExposable
{
    public Color Color;

    public override float CalcAttractionEffect(Pawn pawn)
    {
        if (ColorHelper.DistanceBetweenColors(Color, pawn.story.HairColor) < 0.15f)
        {
            return Value;
        }
        return 0f;
    }

    public override string Label
    {
        get
        {
            foreach (GeneDef hair in AttractionHelper.HairColorGenes)
            {
                if (hair.hairColorOverride == Color)
                {
                    return hair.label;
                }
            }
            return "unknown hair color";
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref Color, "color");
        Scribe_Values.Look(ref Value, "value");
    }

    public float GetValue() => Value;
}