using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public static class LovinHelper
{
    private static readonly List<Pair<float, ThoughtDef>> qualityToThoughtMapping = new()
    {
        new(5f, LovinThoughtDefOf.PP_ThoughtSocial_TranscendentLovin),
        new(3f, LovinThoughtDefOf.PP_ThoughtSocial_ExquisiteLovin),
        new(1.8f, LovinThoughtDefOf.PP_ThoughtSocial_GreatLovin),
        new(1.3f, LovinThoughtDefOf.PP_ThoughtSocial_GoodLovin),
        new(0.7f, LovinThoughtDefOf.PP_ThoughtSocial_OkayLovin),
        new(0.25f, LovinThoughtDefOf.PP_ThoughtSocial_BadLovin),
        new(-1000f, LovinThoughtDefOf.PP_ThoughtSocial_TerribleLovin),
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

    public static readonly SimpleCurve chanceToHookupByRomanceDesire = new()
    {
        new CurvePoint(-1f, 1.5f),
        new CurvePoint(1f, 0.5f),
    };

    private static readonly SimpleCurve acceptanceRollOffsetByLovinNeed = new()
    {
        new CurvePoint(0.75f, 0f),
        new CurvePoint(0f, 0.5f)
    };

    public static readonly SimpleCurve lovinQualityFactorByOpinion = new()
    {
        new CurvePoint(100f, 1.5f),
        new CurvePoint(20f, 1f),
        new CurvePoint(-20f, .75f),
        new CurvePoint(-100f, 1.5f)
    };

    private static readonly List<Pair<ThoughtDef, Func<QuirkDef, float>>> cheatingThoughtPairs = new()
    {
        new(LovinThoughtDefOf.PP_CheatedGuilty, (QuirkDef quirk) => Mathf.Clamp(2f - quirk.statFactors[0].value, 0, 2f)),
        new(LovinThoughtDefOf.PP_CheatedDontCare, (QuirkDef _) => 1f),
        new(LovinThoughtDefOf.PP_CheatedHappy, (QuirkDef quirk) => Mathf.Clamp(quirk.statFactors[0].value - 1f, 0, 2f))
    };

    public static void ResetLovinCooldown(Pawn pawn)
    {
        RomanceComp comp = pawn.GetComp<RomanceComp>();
        if (pawn.health.hediffSet.HasHediff(LovinDefOf.PP_SeductionPheromones))
        {
            // set the cd much lower if the pawn is seduced
            comp.LovinCooldownTicksRemaining = Mathf.FloorToInt(GenDate.TicksPerHour * 0.25f);
        }
        else
        {
            comp.LovinCooldownTicksRemaining = GenDate.TicksPerHour * 2;
        }
    }

    public static Job TryDoSelfLovin(Pawn pawn)
    {
        Building_Bed bed = FindBed(pawn);
        if (bed == null)
        {
            return null;
        }
        ResetLovinCooldown(pawn);
        return JobMaker.MakeJob(LovinJobDefOf.DoSelfLovin, bed, bed.GetSleepingSlotPos(0));
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
            ClearSeducedHediff(props.Actor);
            return;
        }

        if (props.Partner == null) return;

        MindComp actorMind = props.Actor.GetComp<MindComp>();

        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.GotLovin, props.Actor.Named(HistoryEventArgsNames.Doer)));

        bool actorIsCheating = RelationshipHelper.WouldBeCheating(props.Actor, props.Partner);
        if (actorIsCheating)
        {
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(LovinEventDefOf.PP_CheatedOnPartner, props.Actor.Named(HistoryEventArgsNames.Doer)));

            Quirk cheatingQuirk = actorMind.GetFirstQuirkInCategory(LovinQuirkDefOf.PP_Fidelity);
            ThoughtDef thoughtDef = cheatingThoughtPairs.RandomElementByWeight((el) => el.Second(cheatingQuirk.Def)).First;

            Thought_Memory thought = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
            props.Actor.TryGiveThought(thought);
        }

        if (props.Actor.IsLoveFeeder())
        {
            SuccubiHelper.OffsetVitality(props.Actor, 0.25f);
        }
        if (props.Partner.IsLoveFeeder())
        {
            Find.HistoryEventsManager.RecordEvent(new HistoryEvent(LovinEventDefOf.PP_GotLovinSuccubus, props.Actor.Named(HistoryEventArgsNames.Doer)));

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

        // wrap this in a check for isInitiator so that we can get the quality for both partners in
        // the same place and only create a single lovin' journal entry
        if (props.IsInitiator)
        {
            float actorQuality = MakeSatisfaction(props.Actor, props.Partner, props.Context);
            float partnerQuality = MakeSatisfaction(props.Partner, props.Actor, props.Context);

            bool isCheating = actorIsCheating || RelationshipHelper.WouldBeCheating(props.Partner, props.Actor);

            LovinTrackerComp journal = Current.Game.GetComponent<LovinTrackerComp>();
            journal.AddEvent(props, actorQuality, partnerQuality, isCheating);
        }

        ClearSeducedHediff(props.Actor, props.Partner);

        RomanceComp romanceComp = props.Actor.GetComp<RomanceComp>();
        romanceComp?.UpdateLovinJournal();
    }

    private static void ClearSeducedHediff(Pawn actor, Pawn partner = null)
    {
        Hediff actorHediff = actor.health.hediffSet.GetFirstHediffOfDef(LovinDefOf.PP_SeductionPheromones);
        if (actorHediff != null)
        {
            actor.health.RemoveHediff(actorHediff);
        }

        if (partner == null) return;
        Hediff partnerHediff = partner.health.hediffSet.GetFirstHediffOfDef(LovinDefOf.PP_SeductionPheromones);
        if (partnerHediff != null)
        {
            partner.health.RemoveHediff(partnerHediff);
        }
    }

    private static float MakeSatisfaction(Pawn primary, Pawn partner, LovinContext context)
    {
        float quality = GetLovinQuality(primary, partner, context);
        primary.IncreaseLovinNeed(quality);
        if (primary.needs.joy != null)
        {
            primary.needs.joy.CurLevel += Mathf.Clamp(quality * 0.25f, 0, 0.5f);
        }

        ThoughtDef thoughtDef = GetLovinThought(quality);
        if (thoughtDef != null)
        {
            primary.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, partner);
        }
        return quality;
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

        quality += partnerSkill + ownSkill * 0.1f;

        RomanceComp romanceComp = primary.GetComp<RomanceComp>();
        AttractionEvaluation attraction = romanceComp.AttractionTracker.GetEvalFor(partner);

        switch (context)
        {
            case LovinContext.Casual:
                quality *= Mathf.Clamp(attraction.PhysicalScore + attraction.PersonalityScore * 0.25f, 0.75f, 1.5f);
                break;

            case LovinContext.Intimate:
                quality *= lovinQualityFactorByOpinion.Evaluate(primary.relations.OpinionOf(partner));
                break;

            case LovinContext.Seduced:
                if (primary.health.hediffSet.HasHediff(LovinDefOf.PP_SeductionPheromones))
                {
                    quality *= 1.5f;
                }
                break;
        }

        return quality;
    }

    public static void MakeLovinMessage(Pawn actor, Pawn partner, List<Pawn> existingPartners, bool isCheating, JobDef job)
    {
        if (isCheating)
        {
            Messages.Message(
                "PP.CheatingNotification".Translate(actor.Named("PAWN"), existingPartners[0].Named("LOVER"), partner.Named("PARTNER")),
                new LookTargets(actor),
                MessageTypeDefOf.NeutralEvent
                );
            return;
        }
        if (job == LovinJobDefOf.LeadHookup)
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

            if (pawn.guest?.GuestStatus == GuestStatus.Prisoner || pawn.guest?.GuestStatus == GuestStatus.Slave) continue;

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
        float roll = Rand.Value;

        // TODO add in relationship checks (existing lovers are much more likely to accept, etc)

        // TODO add precept checks: unmarried pawns in non-free-lovin ideos are unlikely to accept,
        // depending on strength of precept

        // the lower the target's lovin' need, the more likely they are to accept lovin'
        Need targetNeed = target.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);
        if (targetNeed != null && targetNeed.CurLevel < 0.75f)
        {
            roll += acceptanceRollOffsetByLovinNeed.Evaluate(targetNeed.CurLevel);
        }

        // target is much less likely to accept if they have an orientation mismatch
        if (!SexualityHelper.DoesOrientationMatch(actor, target, true))
        {
            roll *= .1f;
        }
        else
        {
            // use clamped attraction if sexuality matches
            RomanceComp targetComp = target.GetComp<RomanceComp>();
            AttractionEvaluation eval = targetComp?.AttractionTracker?.GetEvalFor(actor);
            if (eval != null)
            {
                roll += Mathf.Clamp(eval.PhysicalScore * 0.5f, -0.5f, 0.5f);
            }
        }

        // a pawn who cares about cheating and is in a relationship is less likely to accept a
        // hookup from someone they aren't in a relationship with

        if (RelationshipHelper.WouldBeCheating(target, actor))
        {
            roll *= target.GetStatValue(LovinDefOf.PP_CheatingLikelihood);
        }

        if (roll >= (1 - acceptanceRate))
        {
            return true;
        }
        return false;
    }

    public static bool DoesTargetAcceptIntimacy(Pawn actor, Pawn target)
    {
        float acceptanceRate = 0.75f;
        float roll = Rand.Value;

        // the lower the target's lovin' need, the more likely they are to accept lovin'
        Need targetNeed = target.needs.TryGetNeed(LovinDefOf.PP_Need_Lovin);
        if (targetNeed != null && targetNeed.CurLevel < 0.75f)
        {
            roll += acceptanceRollOffsetByLovinNeed.Evaluate(targetNeed.CurLevel);
        }

        // target is much less likely to accept if they have an orientation mismatch
        if (!SexualityHelper.DoesOrientationMatch(actor, target, true))
        {
            roll *= .1f;
        }

        if (RelationshipHelper.WouldBeCheating(target, actor))
        {
            roll *= target.GetStatValue(LovinDefOf.PP_CheatingLikelihood);
        }

        if (roll >= acceptanceRate)
        {
            return true;
        }
        return false;
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

    public static bool CheckForPregnancy(Pawn pawn, Pawn partner, float basePregChance)
    {
        // leaving these separated out in case I decided to add some more modifiers later
        float updatedChance = basePregChance * pawn.GetStatValue(StatDefOf.Fertility) * partner.GetStatValue(StatDefOf.Fertility);
        return Rand.Value < updatedChance;
    }

    public static void TryPregnancy(LovinProps props)
    {
        if (props.Actor.gender == Gender.Female && props.Partner.gender == Gender.Male)
        {
            bool gotPregnant = CheckForPregnancy(props.Actor, props.Partner, .05f);
            if (gotPregnant)
            {
                GeneSet geneSet = PregnancyUtility.GetInheritedGeneSet(props.Partner, props.Actor, out bool ableToGenerateBaby);
                if (ableToGenerateBaby)
                {
                    Hediff_Pregnant pregnancy = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, props.Actor);
                    pregnancy.SetParents(null, props.Partner, geneSet);
                    props.Actor.health.AddHediff(pregnancy);
                }
            }
        }
    }
}