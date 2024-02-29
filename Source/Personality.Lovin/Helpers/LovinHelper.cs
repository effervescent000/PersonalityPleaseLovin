using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace Personality.Lovin;

public static class LovinHelper
{
    private const float MINIMUM_HOOKUP_ACCEPTANCE_VALUE = 0.5f;

    private static readonly List<string> romanticRelationDefs = new() { PawnRelationDefOf.Lover.defName, PawnRelationDefOf.Fiance.defName, PawnRelationDefOf.Spouse.defName };

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

    public static readonly SimpleCurve LovinNeedFallByPurityCurve = new()
    {
        new CurvePoint(-1f, 2f),
        new CurvePoint(0f, 1f),
        new CurvePoint(1f, 0.5f),
    };

    private static readonly SimpleCurve chanceToIgnoreRejectionByLawfulness = new()
    {
        new CurvePoint(-1f, 0.5f),
        new CurvePoint(1f, -0.25f)
    };

    private static readonly SimpleCurve chanceToIgnoreRejectionByCompassion = new()
    {
        new CurvePoint(-1f, 0.5f),
        new CurvePoint(1f, -1f)
    };

    public static Job TryDoSelfLovin(Pawn pawn)
    {
        Building_Bed bed = FindBed(pawn);
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
            bool hasHediff = props.Actor.health.hediffSet.HasHediff(LovinDefOf.PP_VitalityLost);
            if (hasHediff)
            {
                Hediff hediff = props.Actor.health.hediffSet.GetFirstHediffOfDef(LovinDefOf.PP_VitalityLost);
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
        foreach (Pair<float, ThoughtDef> pair in qualityToThoughtMapping)
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

        Dictionary<string, float> attraction = GetAttractionFactorFor(primary, partner);

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
            new CurvePoint(need.Horny, 2f),
            new CurvePoint(need.Desperate, 5f),
            new CurvePoint(0f, 10f),
        };
        return LovinDesireCurve.Evaluate(need.CurLevel);
    }

    public static Job TrySeekLovin(Pawn pawn)
    {
        JobDef job = LovinDefOf.PP_InitiateIntimateLovin;
        Pawn partner = LovinHelper.FindPartnerForIntimacy(pawn);

        // if partner is null, then obviously we're looking for a hookup. otherwise, we may or may
        // not look for a hookup. For now it's just a straight roll but would like to make it based
        // on pawn's personality and quirks
        if (partner == null || Rand.Value < 0.5f)
        {
            partner = FindPartnerForHookup(pawn);
            job = LovinDefOf.LeadHookup;
        }

        // if we can't actually find a partner, then the pawn either gives up and does something
        // else or does self lovin'
        if (partner == null)
        {
            if (Rand.Value <= .75f)
            {
                return null;
            }
            else
            {
                return TryDoSelfLovin(pawn);
            }
        }

        Building_Bed bed = FindBed(pawn, partner);
        if (bed == null)
        {
            return null;
        }

        return JobMaker.MakeJob(job, partner, bed);
    }

    public static Pawn FindPartnerForIntimacy(Pawn actor)
    {
        List<DirectPawnRelation> relations = actor.relations.DirectRelations;
        List<Pawn> potentialPartners = new();

        if (relations.Count == 0) return null;

        MindComp mind = actor.GetComp<MindComp>();

        float? actorCompassion = mind.GetNode(PersonalityHelper.COMPASSION)?.FinalRating.Value;
        float? actorLawfulness = mind.GetNode(PersonalityHelper.LAWFULNESS)?.FinalRating.Value;

        foreach (DirectPawnRelation rel in relations)
        {
            Pawn target = rel.otherPawn;
            if (!target.Spawned || target.Map.uniqueID != actor.Map.uniqueID) continue;
            if (!romanticRelationDefs.Contains(rel.def.defName)) continue;

            RomanceComp romanceComp = actor.GetComp<RomanceComp>();
            if (romanceComp.RomanceTracker.IsInRejectionList(target))
            {
                float chance = 0f;
                if (actorCompassion != null)
                {
                    chance += chanceToIgnoreRejectionByCompassion.Evaluate((float)actorCompassion);
                }
                if (actorLawfulness != null)
                {
                    chance += chanceToIgnoreRejectionByLawfulness.Evaluate((float)actorLawfulness);
                }

                if (Rand.Value >= chance) continue;
            }

            if (!target.IsOk()) continue;

            potentialPartners.Add(target);
        }

        if (potentialPartners.Count > 0)
        {
            if (potentialPartners.Count == 1) { return potentialPartners[0]; }

            List<Pair<Pawn, int>> partnersByAttraction = new();
            foreach (Pawn pawn in potentialPartners)
            {
                partnersByAttraction.Add(new(pawn, actor.relations.OpinionOf(pawn)));
            }
            List<Pair<Pawn, int>> sorted = partnersByAttraction.OrderByDescending(pair => pair.Second).ToList();

            // TODO instead of just choosing the first one, choose weighted random

            //return sorted[0].First;
            return sorted.RandomElementByWeight(pair => pair.Second).First;
        }

        return null;
    }

    public static Pawn FindPartnerForHookup(Pawn actor)
    {
        List<Pawn> availablePawns =
            (
                from pawn in actor.Map.mapPawns.AllPawnsSpawned
                where pawn.def.defName == "Human" && pawn.ageTracker.AgeBiologicalYears >= 16
                select pawn
             ).ToList();

        List<Pawn> potentialPartners = new();

        if (availablePawns.Count == 0) return null;

        MindComp mind = actor.GetComp<MindComp>();

        float? actorCompassion = mind.GetNode(PersonalityHelper.COMPASSION)?.FinalRating.Value;
        float? actorLawfulness = mind.GetNode(PersonalityHelper.LAWFULNESS)?.FinalRating.Value;

        foreach (Pawn pawn in availablePawns)
        {
            if (pawn.ThingID == actor.ThingID || !pawn.IsOk()) continue;
            if (!SexualityHelper.DoesOrientationMatch(actor, pawn, true)) continue;
            if (!GeneralHelper.IsTargetInRange(actor, pawn)) continue;

            RomanceComp comp = actor.GetComp<RomanceComp>();
            if (comp.RomanceTracker.IsInRejectionList(pawn))
            {
                float chance = 0f;
                if (actorCompassion != null)
                {
                    chance += chanceToIgnoreRejectionByCompassion.Evaluate((float)actorCompassion);
                }
                if (actorLawfulness != null)
                {
                    chance += chanceToIgnoreRejectionByLawfulness.Evaluate((float)actorLawfulness);
                }

                if (Rand.Value >= chance) continue;
            }

            if (actor.IsBloodRelatedTo(pawn)) continue;

            potentialPartners.Add(pawn);
        }
        if (potentialPartners.Count > 0)
        {
            List<Pair<Pawn, AttractionEvaluation>> partnersByAttraction = new();
            RomanceComp comp = actor.GetComp<RomanceComp>();
            foreach (Pawn partner in potentialPartners)
            {
                partnersByAttraction.Add(new(partner, comp.AttractionTracker.GetEvalFor(partner)));
            }
            List<Pair<Pawn, AttractionEvaluation>> sorted = partnersByAttraction.OrderByDescending(pair => pair.Second.PhysicalScore).ToList();
            Log.Message($"returning partner {sorted[0].First.LabelShort} with an attraction of {sorted[0].Second.PhysicalScore}");

            // TODO make personality a non-zero factor in hookups, altho i'm not sure how important
            // to make it

            return sorted.RandomElementByWeight(pair => pair.Second.PhysicalScore).First;
        }
        return null;
    }

    public static bool DoesTargetAcceptHookup(Pawn actor, Pawn target)
    {
        float rolledValue = Rand.Value;

        // TODO add in relationship checks (existing lovers are much more likely to accept, etc)

        // TODO add precept checks: unmarried pawns in non-free-lovin ideos are unlikely to accept,
        // depending on strength of precept

        // target is much less likely to accept if they have an orientation mismatch
        if (!SexualityHelper.DoesOrientationMatch(actor, target, true))
        {
            rolledValue *= .1f;
        }

        if (rolledValue < MINIMUM_HOOKUP_ACCEPTANCE_VALUE)
        {
            return false;
        }
        return true;
    }

    public static Toil FinishLovin(LovinProps props)
    {
        return new Toil
        {
            initAction = delegate
            {
                EvaluateLovin(props);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }

    public static Building_Bed FindBed(Pawn actor, Pawn partner = null)
    {
        // find literally any bed
        List<Building_Bed> beds = actor.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().ToList();
        if (beds.Count > 0)
        {
            return beds[0];
        }
        return null;
    }

    public static bool IsInOrByBed(Building_Bed bed, Pawn pawn)
    {
        for (int i = 0; i < bed.SleepingSlotsCount; i++)
        {
            if (bed.GetSleepingSlotPos(i).InHorDistOf(pawn.Position, 1f))
            {
                return true;
            }
        }
        return false;
    }
}