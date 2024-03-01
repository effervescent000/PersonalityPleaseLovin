using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Personality.Lovin;

public class Need_Lovin : Need_Seeker
{
    private readonly float baseFallPerDay = 0.5f;

    // eventually allow thresholds to be modified by traits and sex drive

    private float threshDesperate = 0.05f;
    private float threshHorny = 0.25f;

    public Need_Lovin(Pawn pawn) : base(pawn)
    {
        threshPercents = new List<float>
        {
            threshDesperate, threshHorny
        };
    }

    // this eventually will actually do math to modify the base rate but for now just return it
    private float FallPerDay => baseFallPerDay;

    public float Horny => threshHorny;
    public float Desperate => threshDesperate;

    public override void NeedInterval()
    {
        if (!pawn.Spawned) { return; }

        if (pawn.IsAsexual() || pawn.ageTracker.AgeBiologicalYears < 16)
        {
            CurLevel = 0.5f;
            return;
        }

        float fallPerInterval = (FallPerDay * (float)(1f / GenDate.TicksPerDay)) * 150f;

        MindComp comp = pawn.GetComp<MindComp>();

        float? purityValue = comp.GetNode(PersonalityHelper.PURITY)?.FinalRating.Value;
        if (purityValue != null)
        {
            fallPerInterval *= LovinHelper.LovinNeedFallByPurityCurve.Evaluate((float)purityValue);
        }

        CurLevel -= fallPerInterval;
    }
}