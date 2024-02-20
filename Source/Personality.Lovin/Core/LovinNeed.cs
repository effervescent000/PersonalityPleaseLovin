using Personality.Romance;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class LovinNeed : Need_Seeker
{
    private readonly float baseFallPerDay = 0.33f;

    // eventually allow thresholds to be modified by traits and sex drive

    private float threshDesperate = 0.05f;
    private float threshHorny = 0.25f;

    public LovinNeed(Pawn pawn) : base(pawn)
    {
        threshPercents = new List<float>
        {
            threshDesperate, threshHorny
        };
    }

    // this eventually will actually do math to modify the base rate but for now just return it
    private float FallPerDay => baseFallPerDay;

    public override void NeedInterval()
    {
        if (pawn.IsAsexual() || pawn.ageTracker.AgeBiologicalYears < 16)
        {
            CurLevel = 0.5f;
            return;
        }

        float fallPerInterval = (FallPerDay * (float)(1f / GenDate.TicksPerDay)) * 150f;

        // math goes here

        CurLevel -= fallPerInterval;
    }
}