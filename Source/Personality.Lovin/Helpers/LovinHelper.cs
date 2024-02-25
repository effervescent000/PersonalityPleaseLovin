using Personality.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace Personality.Lovin;

public static class LovinHelper
{
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
            props.Actor.IncreaseLovinNeed(1f);
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
        primary.IncreaseLovinNeed(quality);
        primary.needs.joy.CurLevel += quality * 0.5f;

        // add to lovin' journal
    }

    public static float GetLovinQuality(Pawn primary, Pawn partner, LovinContext context)
    {
        float quality = 0f;

        float partnerSkill = partner.GetStatValue(LovinDefOf.LovinQuality);
        float ownSkill = primary.GetStatValue(LovinDefOf.LovinQuality);

        // TODO -- in an intimate context, look at the pawns' relationship -- higher boosts lovin'

        // TODO -- in a casual context, look at each pawns' attraction to the other

        quality += partnerSkill + ownSkill * 0.25f;

        return quality;
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