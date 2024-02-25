using Personality.Core;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public static class LovinHelper
{
    private static readonly List<Pair<float, ThoughtDef>> qualityToThoughtMapping = new()
    {
        new(5f, LovinDefOf.PP_ThoughtSocial_TranscendentLovin),
        new(3f, LovinDefOf.PP_ThoughtSocial_ExquisiteLovin),
        new(1.8f, LovinDefOf.PP_ThoughtSocial_GreatLovin),
        new(1.3f, LovinDefOf.PP_ThoughtSocial_GoodLovin),
        new(0.7f, LovinDefOf.PP_ThoughtSocial_OkayLovin),
        new(0.25f, LovinDefOf.PP_ThoughtSocial_BadLovin),
        new(0f, LovinDefOf.PP_ThoughtSocial_TerribleLovin),
    };

    public static Job TryDoSelfLovin(Pawn pawn)
    {
        Building_Bed bed = CoreLovinHelper.FindBed(pawn);
        if (bed == null)
        {
            return null;
        }
        return JobMaker.MakeJob(LovinDefOf.DoSelfLovin, bed, bed.GetSleepingSlotPos(0));
    }

    public static void IncreaseLovinNeed(this Pawn pawn, float amount)
    {
        Need_Lovin need = pawn?.needs?.TryGetNeed<Need_Lovin>();
        if (need == null) return;

        need.CurLevel += amount;
    }

    public static void EvaluateLovin(LovinProps props)
    {
        if (props.Actor == null) return;

        if (props.Context == LovinContext.SelfLovin)
        {
            props.Actor.IncreaseLovinNeed(0.4f);
            return;
        }

        if (props.Partner == null) return;

        if (props.Actor.IsLoveFeeder())
        {
            SuccubiHelper.OffsetVitality(props.Actor, 0.25f);
        }
        if (props.Partner.IsLoveFeeder())
        {
            var hasHediff = props.Actor.health.hediffSet.HasHediff(LovinDefOf.PP_VitalityLost);
            if (hasHediff)
            {
                var hediff = props.Actor.health.hediffSet.GetFirstHediffOfDef(LovinDefOf.PP_VitalityLost);
                hediff.Severity += 0.25f;
            }
            else
            {
                props.Actor.health.AddHediff(HediffMaker.MakeHediff(LovinDefOf.PP_VitalityLost, props.Actor));
            }
        }

        // we only want to run this once, as it will (I think) run once for each pawn at the end of
        // their respective jobs
        MakeSatisfaction(props.Actor, props.Partner, props.Context);
    }

    private static void MakeSatisfaction(Pawn primary, Pawn partner, LovinContext context)
    {
        float quality = GetLovinQuality(primary, partner, context);
        Log.Message($"Loving quality for {primary.LabelShort} of {quality}");
        primary.IncreaseLovinNeed(quality);
        primary.needs.joy.CurLevel += quality * 0.5f;

        ThoughtDef thoughtDef = GetLovinThought(quality);
        if (thoughtDef != null)
        {
            primary.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, partner);
        }

        // add to lovin' journal
    }

    public static ThoughtDef GetLovinThought(float quality)
    {
        foreach (var pair in qualityToThoughtMapping)
        {
            if (quality >= pair.First) return pair.Second;
        }
        return null;
    }

    public static float GetLovinQuality(Pawn primary, Pawn partner, LovinContext context)
    {
        float quality = 0f;

        float partnerSkill = partner.GetStatValue(LovinDefOf.LovinQuality);
        float ownSkill = primary.GetStatValue(LovinDefOf.LovinQuality);

        // TODO -- in an intimate context, look at the pawns' relationship -- higher boosts lovin'

        quality += partnerSkill + ownSkill * 0.25f;

        var attraction = GetAttractionFactorFor(primary, partner);

        switch (context)
        {
            case LovinContext.Casual:
                if (attraction.TryGetValue("physical", out float physical) && attraction.TryGetValue("personality", out float personality))
                {
                    quality *= Mathf.Clamp(physical + personality, 0.75f, 1.5f);
                }
                break;

            case LovinContext.Seduced:
                // TODO seducee gets a decent bump to their lovin' received quality. who is the
                // seducee will probably be determined by a "seduced" hediff that does not yet exist
                // (needs to be this since one succubus could potentially seduce another)
                break;
        }

        return quality;
    }

    public static Dictionary<string, float> GetAttractionFactorFor(Pawn pawn, Pawn target)
    {
        return new() { { "physical", 1f }, { "personality", 1f } };
    }

    public static float GetChanceToSeekLovin(Pawn pawn)
    {
        Need_Lovin need = (Need_Lovin)pawn.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);

        //initialize a curve based on the pawn's lovin need thresholds. if a pawn is at 100% lovin' need, they will never seek lovin'.

        SimpleCurve LovinDesireCurve = new()
        {
            new CurvePoint(1f, 0f),
            new CurvePoint(need.Horny, 3f),
            new CurvePoint(need.Desperate, 10f)
        };
        return LovinDesireCurve.Evaluate(need.CurLevel);
    }
}