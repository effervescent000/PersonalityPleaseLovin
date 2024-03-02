using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public static class LovinHelper
{
    private static readonly List<string> romanticRelationDefs = new() { PawnRelationDefOf.Lover.defName, PawnRelationDefOf.Fiance.defName, PawnRelationDefOf.Spouse.defName };

    private static readonly List<Pair<float, ThoughtDef>> qualityToThoughtMapping = new()
    {
        new(5f, LovinThoughtDefOf.PP_ThoughtSocial_TranscendentLovin),
        new(3f, LovinThoughtDefOf.PP_ThoughtSocial_ExquisiteLovin),
        new(1.8f, LovinThoughtDefOf.PP_ThoughtSocial_GreatLovin),
        new(1.3f, LovinThoughtDefOf.PP_ThoughtSocial_GoodLovin),
        new(0.7f, LovinThoughtDefOf.PP_ThoughtSocial_OkayLovin),
        new(0.25f, LovinThoughtDefOf.PP_ThoughtSocial_BadLovin),
        new(0f, LovinThoughtDefOf.PP_ThoughtSocial_TerribleLovin),
    };

    public static readonly SimpleCurve LovinNeedFallByPurityCurve = new()
    {
        new CurvePoint(-1f, 1.5f),
        new CurvePoint(0f, 1f),
        new CurvePoint(1f, 0.75f),
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

    private static readonly SimpleCurve chanceToHookupByRomanceDesire = new()
    {
        new CurvePoint(-1f, 1.5f),
        new CurvePoint(1f, 0.5f),
    };

    private static readonly SimpleCurve chanceToCheatByFidelity = new()
    {
        new CurvePoint(-1f, 1.5f),
        new CurvePoint(1f, 0.01f)
    };

    private static readonly SimpleCurve acceptanceOffsetByLovinNeed = new()
    {
        new CurvePoint(0.75f, 0f),
        new CurvePoint(0f, 0.5f)
    };

    public static void ResetLovinCooldown(Pawn pawn)
    {
        RomanceComp comp = pawn.GetComp<RomanceComp>();
        comp.LovinCooldownTicksRemaining = GenDate.TicksPerHour * 2;
    }

    public static Job TryDoSelfLovin(Pawn pawn)
    {
        Building_Bed bed = FindBed(pawn);
        if (bed == null)
        {
            return null;
        }
        ResetLovinCooldown(pawn);
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
            props.Actor.needs.joy.CurLevel += 0.2f;
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

        SimpleCurve LovinNeedCurve = new()
        {
            new CurvePoint(1f, 0f),
            new CurvePoint(need.Horny, 4f),
            new CurvePoint(need.Desperate, 8f),
            new CurvePoint(0f, 12f),
        };
        return LovinNeedCurve.Evaluate(need.CurLevel);
    }

    public static Job TrySeekLovin(Pawn pawn)
    {
        MindComp mind = pawn.GetComp<MindComp>();

        JobDef job = LovinDefOf.PP_InitiateIntimateLovin;
        Pawn partner = FindPartnerForIntimacy(pawn, mind);

        bool isCheating = false;
        Pawn existingPartner = null;

        // if partner is null, then obviously we're looking for a hookup. otherwise, calculate the
        // roll for a hookup
        if (partner == null)
        {
            partner = FindPartnerForHookup(pawn, mind);
            job = LovinDefOf.LeadHookup;
        }
        else
        {
            float hookupThreshold = 0.5f;

            //if a pawn is monogamous and partnered, calculate effect of fidelity
            if (mind.GetQuirkByDef(LovinQuirkDefOf.PP_Monogamous, out Quirk _) && pawn.IsPartnered(out existingPartner))
            {
                hookupThreshold -= 0.2f;
                Quirk fidelity = mind.GetOrGainQuirkSingular(LovinQuirkDefOf.PP_Fidelity);
                hookupThreshold *= chanceToCheatByFidelity.Evaluate(fidelity.Value);

                // lastly add cheating multiplier from settings
                hookupThreshold *= LovinMod.Settings.CheatingModifier.Value / 100f;
            }

            if (mind.GetQuirkByDef(LovinQuirkDefOf.PP_RomanceSeeking, out Quirk romanceDesire))
            {
                hookupThreshold *= chanceToHookupByRomanceDesire.Evaluate(romanceDesire.Value);
            }

            if (Rand.Value < hookupThreshold)
            {
                partner = FindPartnerForHookup(pawn, mind);
                job = LovinDefOf.LeadHookup;
                if (existingPartner != null && existingPartner.ThingID != partner.ThingID) isCheating = true;
            }
        }

        // if we can't actually find a partner, then the pawn either gives up and does something
        // else or does self lovin'
        if (partner == null)
        {
            if (Rand.Value < .75f)
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

        MakeLovinMessage(pawn, partner, existingPartner, isCheating, job);
        ResetLovinCooldown(pawn);
        return JobMaker.MakeJob(job, partner, bed);
    }

    private static void MakeLovinMessage(Pawn actor, Pawn partner, Pawn existingPartner, bool isCheating, JobDef job)
    {
        if (isCheating)
        {
            Messages.Message(
                "PP.CheatingNotification".Translate(actor.Named("PAWN"), existingPartner.Named("LOVER"), partner.Named("PARTNER")),
                new LookTargets(actor),
                MessageTypeDefOf.NeutralEvent
                );
            return;
        }
        if (job == LovinDefOf.LeadHookup)
        {
            Messages.Message(
                "PP.HookupNotification".Translate(actor.Named("PAWN"), partner.Named("PARTNER")),
                new LookTargets(actor),
                MessageTypeDefOf.NeutralEvent
                );
            return;
        }

        // for now, if we get to this point, it's always intimate lovin, but this will need to be
        // tweaked in the future
        Messages.Message(
                "PP.IntimacyNotification".Translate(actor.Named("PAWN"), partner.Named("PARTNER")),
                new LookTargets(actor),
                MessageTypeDefOf.NeutralEvent
                );
        return;
    }

    public static Pawn FindPartnerForIntimacy(Pawn actor, MindComp mind)
    {
        List<DirectPawnRelation> relations = actor.relations.DirectRelations;
        List<Pawn> potentialPartners = new();

        if (relations.Count == 0) return null;

        float? actorCompassion = mind.GetNode(PersonalityHelper.COMPASSION)?.FinalRating.Value;
        float? actorLawfulness = mind.GetNode(PersonalityHelper.LAWFULNESS)?.FinalRating.Value;

        foreach (DirectPawnRelation rel in relations)
        {
            Pawn target = rel.otherPawn;
            if (!target.Spawned || target.Map.uniqueID != actor.Map.uniqueID) continue;
            if (!RelationshipHelper.romanticRelationDefs.Contains(rel.def)) continue;

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

            return sorted.RandomElementByWeight(pair => pair.Second).First;
        }

        return null;
    }

    public static Pawn FindPartnerForHookup(Pawn actor, MindComp mind)
    {
        List<Pawn> availablePawns =
            (
                from pawn in actor.Map.mapPawns.AllPawnsSpawned
                where pawn.def.defName == "Human" && pawn.ageTracker.AgeBiologicalYears >= 16
                select pawn
             ).ToList();

        List<Pawn> potentialPartners = new();

        if (availablePawns.Count == 0) return null;

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

            // add 100 to all weights to prevent scores from going negative
            return sorted.RandomElementByWeight(pair => pair.Second.PhysicalScore + 100f).First;
        }
        return null;
    }

    public static bool DoesTargetAcceptHookup(Pawn actor, Pawn target)
    {
        float acceptanceRate = 0.5f;

        // TODO add in relationship checks (existing lovers are much more likely to accept, etc)

        // TODO add precept checks: unmarried pawns in non-free-lovin ideos are unlikely to accept,
        // depending on strength of precept

        // the lower the target's lovin' need, the more likely they are to accept lovin'
        Need targetNeed = target.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);
        if (targetNeed != null && targetNeed.CurLevel < 0.75f)
        {
            acceptanceRate += acceptanceOffsetByLovinNeed.Evaluate(targetNeed.CurLevel);
        }

        // target is much less likely to accept if they have an orientation mismatch
        if (!SexualityHelper.DoesOrientationMatch(actor, target, true))
        {
            acceptanceRate *= .1f;
        }
        else
        {
            // use clamped attraction if sexuality matches
            RomanceComp targetComp = target.GetComp<RomanceComp>();
            var eval = targetComp?.AttractionTracker?.GetEvalFor(actor);
            if (eval != null)
            {
                acceptanceRate += Mathf.Clamp(eval.PhysicalScore * 0.5f, -0.5f, 0.5f);
            }
        }

        if (Rand.Value < acceptanceRate)
        {
            return true;
        }
        return false;
    }

    public static bool DoesTargetAcceptIntimacy(Pawn actor, Pawn target)
    {
        float acceptanceRate = 0.75f;

        // the lower the target's lovin' need, the more likely they are to accept lovin'
        Need targetNeed = target.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);
        if (targetNeed != null && targetNeed.CurLevel < 0.75f)
        {
            acceptanceRate += acceptanceOffsetByLovinNeed.Evaluate(targetNeed.CurLevel);
        }

        // target is much less likely to accept if they have an orientation mismatch
        if (!SexualityHelper.DoesOrientationMatch(actor, target, true))
        {
            acceptanceRate *= .1f;
        }

        if (Rand.Value < acceptanceRate)
        {
            return true;
        }
        return false;
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
        int spotsNeeded = partner != null ? 2 : 1;

        // first try and get the bed of either the actor or partner
        Building_Bed actorBed = actor.ownership.OwnedBed;
        if (IsBedValid(actorBed, spotsNeeded)) return actorBed;

        if (partner != null)
        {
            Building_Bed partnerBed = partner.ownership.OwnedBed;
            if (IsBedValid(partnerBed, spotsNeeded)) return partnerBed;
        }

        // if neither bed is valid, just try and find any unoccupied bed
        List<Building_Bed> beds = actor.Map.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>().ToList();
        if (beds.Count > 0)
        {
            foreach (Building_Bed bed in beds)
            {
                if (IsBedValid(bed, spotsNeeded) && RestUtility.CanUseBedEver(actor, bed.def))
                {
                    if (partner == null)
                    {
                        return bed;
                    }
                    if (RestUtility.CanUseBedEver(partner, bed.def))
                    {
                        return bed;
                    }
                }
            }
        }
        return null;
    }

    private static bool IsBedValid(Building_Bed bed, int neededSpots)
    {
        if (bed == null) return false;
        if (bed.SleepingSlotsCount < neededSpots)
        {
            return false;
        }
        if (bed.AnyOccupants)
        {
            return false;
        }
        return true;
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