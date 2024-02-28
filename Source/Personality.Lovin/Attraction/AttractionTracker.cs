using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Personality.Lovin;

public class AttractionTracker : IExposable
{
    public Pawn pawn;

    public List<Preference> HairStylePreferences = new();
    public List<Preference> BodyPreferences = new();
    public List<Preference> HairColorPreferences = new();
    public List<Preference> HeadTypePreferences = new();

    public List<Preference> AllPrefs = new();

    public Dictionary<string, AttractionEvaluation> evals = new();

    public AttractionTracker(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public void Initialize()
    {
        int seed = pawn.GetSeed();
        System.Random random = new(seed);

        while (HairStylePreferences.Count < 2)
        {
            List<string> currentHairTags = (from tag in HairStylePreferences
                                            select tag.Label).ToList();

            string newTag = AttractionHelper.HairStyleTags.RandomElement();
            if (!currentHairTags.Contains(newTag))
            {
                HairStylePreferences.Add(new PreferenceHairStyle { Style = newTag, Value = GetUnmoderateValue(random) });
            }
        }

        List<BodyTypeDef> validBodyTypes = MakeBodyTypes(pawn);

        while (BodyPreferences.Count < Math.Floor(validBodyTypes.Count * 0.4f))
        {
            List<string> currentPrefs = (from body in BodyPreferences
                                         select body.Label).ToList();
            BodyTypeDef selection = validBodyTypes.RandomElement();
            if (!currentPrefs.Contains(selection.defName))
            {
                BodyPreferences.Add(new PreferenceBodyType { Def = selection, Value = GetUnmoderateValue(random) });
            }
        }

        while (HairColorPreferences.Count < 2)
        {
            List<Color> currentPrefs = (from PreferenceHairColor color in HairColorPreferences
                                        select color.Color).ToList();
            GeneDef selection = AttractionHelper.HairColorGenes.RandomElement();
            if (!currentPrefs.Contains((Color)selection.hairColorOverride))
            {
                HairColorPreferences.Add(new PreferenceHairColor { Color = (Color)selection.hairColorOverride, Value = GetUnmoderateValue(random) });
            }
        }

        // give pawns one head type attraction for each gender they're attracted to
        if (pawn.IsAttractedToMen())
        {
            HeadTypeDef selection = AttractionHelper.MaleHeads.RandomElement();
            HeadTypePreferences.Add(new PreferenceHeadType { Def = selection, Value = GetUnmoderateValue(random) });
        }
        if (pawn.IsAttractedToWomen())
        {
            HeadTypeDef selection = AttractionHelper.FemaleHeads.RandomElement();
            HeadTypePreferences.Add(new PreferenceHeadType { Def = selection, Value = GetUnmoderateValue(random) });
        }

        // at the very end
        AllPrefs = AllPrefs.Concat(BodyPreferences).Concat(HairColorPreferences).Concat(HeadTypePreferences).Concat(HairStylePreferences).ToList();
    }

    private float GetUnmoderateValue(System.Random rand)
    {
        while (true)
        {
            float newValue = rand.Next(-100, 100);
            if (newValue < 25)
            {
                if (newValue > -25)
                {
                    continue;
                }
            }
            return newValue / 100f;
        }
    }

    private List<BodyTypeDef> MakeBodyTypes(Pawn pawn)
    {
        List<BodyTypeDef> validBodyTypes = AttractionHelper.GenericBodyTypes.ListFullCopy();

        if (pawn.IsAttractedToMen())
        {
            validBodyTypes.Add(BodyTypeDefOf.Male);
        }
        if (pawn.IsAttractedToWomen())
        {
            validBodyTypes.Add(BodyTypeDefOf.Female);
        }

        return validBodyTypes;
    }

    public AttractionEvaluation GetEvalFor(Pawn pawn)
    {
        if (evals.TryGetValue(pawn.ThingID, out AttractionEvaluation eval))
        {
            return eval;
        }
        AttractionEvaluation newEval = new(pawn);
        newEval.MakeEval(this);
        return newEval;
    }

    public void Tick()
    {
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref HairStylePreferences, "hairStylePrefs", LookMode.Deep);
        Scribe_Collections.Look(ref BodyPreferences, "bodyPreferences", LookMode.Deep);
        Scribe_Collections.Look(ref HairColorPreferences, "colorPreferences", LookMode.Deep);
    }
}