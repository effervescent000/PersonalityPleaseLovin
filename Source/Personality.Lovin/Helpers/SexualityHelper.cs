using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Personality.Lovin;

public static class SexualityHelper
{
    // to be replaced with stuff from settings
    public const float asexualityChanceBase = 0.05f;

    public const float homoChanceBase = 0.225f;
    public const float biChanceBase = 0.50f;
    public const float heteroChanceBase = 0.225f;

    public static SexualityValues straightValues = new(LovinDefOf.Straight, heteroChanceBase);
    public static SexualityValues biValues = new(TraitDefOf.Bisexual, biChanceBase);
    public static SexualityValues gayValues = new(TraitDefOf.Gay, homoChanceBase);

    public static SexualityValues aceValues = new(TraitDefOf.Asexual, asexualityChanceBase);
    public static SexualityValues aceBiValues = new(LovinDefOf.AceBi, biChanceBase);
    public static SexualityValues aceHeteroValues = new(LovinDefOf.AceHetero, heteroChanceBase);
    public static SexualityValues aceHomoValues = new(LovinDefOf.AceHomo, homoChanceBase);
    public static SexualityValues aroAceValues = new(LovinDefOf.AroAce, asexualityChanceBase);

    public static List<string> asexualTraitDefNames = new()
        {   aceBiValues.TraitDef.defName,
            aceHeteroValues.TraitDef.defName,
            aceHomoValues.TraitDef.defName,
            aroAceValues.TraitDef.defName
        };

    public static List<string> biTraitDefNames = new() { TraitDefOf.Bisexual.defName, LovinDefOf.AceBi.defName };
    public static List<string> heteroTraitDefNames = new() { LovinDefOf.Straight.defName, LovinDefOf.AceHetero.defName };
    public static List<string> homoTraitDefNames = new() { TraitDefOf.Gay.defName, LovinDefOf.AceHomo.defName };

    public static void RollSexualityTraitFor(Pawn pawn)
    {
        bool maybeGay = false;
        bool maybeStraight = false;

        List<SexualityValues> rollingForSexuality;

        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn))
        {
            maybeGay = true;
        }
        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
        {
            maybeStraight = true;
        }

        SexualityValues found;

        if (maybeGay && maybeStraight)
        {
            rollingForSexuality = new()
            {
                biValues, aceValues
            };
            found = FindOrientation(rollingForSexuality);
            if (found.TraitDef.defName == TraitDefOf.Asexual.defName)
            {
                pawn.story.traits.GainTrait(new Trait(LovinDefOf.AceBi));
                return;
            }
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Bisexual));
            return;
        }
        if (maybeGay)
        {
            rollingForSexuality = new() { biValues, gayValues, aceValues };
            found = FindOrientation(rollingForSexuality);
            if (found.TraitDef.defName == TraitDefOf.Asexual.defName)
            {
                List<SexualityValues> rollingForAce = new() { aceBiValues, aceHomoValues };
                pawn.story.traits.GainTrait(new Trait(FindOrientation(rollingForAce).TraitDef));
                return;
            }
            pawn.story.traits.GainTrait(new Trait(found.TraitDef));
            return;
        }
        if (maybeStraight)
        {
            rollingForSexuality = new() { biValues, straightValues, aceValues };
            found = FindOrientation(rollingForSexuality);
            if (found.TraitDef.defName == TraitDefOf.Asexual.defName)
            {
                List<SexualityValues> rollingForAce = new() { aceBiValues, aceHeteroValues };
                pawn.story.traits.GainTrait(new Trait(FindOrientation(rollingForAce).TraitDef));
                return;
            }
            pawn.story.traits.GainTrait(new Trait(found.TraitDef));
            return;
        }

        // now, roll general sexuality (no restrictions based on relationships)
        rollingForSexuality = new() { biValues, straightValues, gayValues, aceValues };
        found = FindOrientation(rollingForSexuality);
        if (found.TraitDef.defName == TraitDefOf.Asexual.defName)
        {
            List<SexualityValues> rollingForAce = new() { aceBiValues, aceHeteroValues, aceHomoValues, aroAceValues };
            pawn.story.traits.GainTrait(new Trait(FindOrientation(rollingForAce).TraitDef));
            return;
        }
        pawn.story.traits.GainTrait(new Trait(found.TraitDef));
    }

    private static SexualityValues FindOrientation(List<SexualityValues> values)
    {
        float sumValue = 0;
        values.ForEach(value => sumValue += value.chance);

        float orientationCheckValue = 0f;
        if (sumValue < 1f)
        {
            values.ForEach(value => orientationCheckValue += value.chance / sumValue);
        }
        else
        {
            orientationCheckValue = 1f;
        }

        float orientationValue = Rand.Value;

        foreach (SexualityValues value in values)
        {
            sumValue -= value.chance;
            if (sumValue <= orientationValue)
            {
                return value;
            }
        }
        throw new Exception("No sexuality match found, somehow");
    }

    public static bool IsAsexual(this Pawn pawn)
    {
        if (pawn.story != null && pawn.story.traits != null)
        {
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (asexualTraitDefNames.Contains(trait.def.defName))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsBisexual(this Pawn pawn)
    {
        if (pawn.story != null && pawn.story.traits != null)
        {
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (biTraitDefNames.Contains(trait.def.defName))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsGay(this Pawn pawn)
    {
        if (pawn.story != null && pawn.story.traits != null)
        {
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (homoTraitDefNames.Contains(trait.def.defName))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsStraight(this Pawn pawn)
    {
        if (pawn.story != null && pawn.story.traits != null)
        {
            foreach (Trait trait in pawn.story.traits.allTraits)
            {
                if (heteroTraitDefNames.Contains(trait.def.defName))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool DoesOrientationMatch(Pawn actor, Pawn target, bool asexualityBlocks = false)
    {
        if (asexualityBlocks && actor.IsAsexual()) { return false; }

        if (actor.IsStraight() && actor.gender == target.gender) { return false; }

        if (actor.IsGay() && actor.gender != target.gender) { return false; }

        // if none of the ifs fail, then it's a match
        return true;
    }

    public static bool IsAttractedToMen(this Pawn pawn)
    {
        if (pawn.IsBisexual()) { return true; }
        if (pawn.gender == Gender.Male)
        {
            if (pawn.IsStraight()) { return false; };
            return true;
        }
        if (pawn.IsStraight()) { return true; }
        return false;
    }

    public static bool IsAttractedToWomen(this Pawn pawn)
    {
        if (pawn.IsBisexual()) { return true; }
        if (pawn.gender == Gender.Female)
        {
            if (pawn.IsStraight()) { return false; };
            return true;
        }
        if (pawn.IsStraight()) { return true; }
        return false;
    }
}