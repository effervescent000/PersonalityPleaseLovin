﻿using RimWorld;
using UnityEngine;

namespace Personality.Lovin;

public class JobDriver_DoLovinCasual : JobDriver_DoLovin
{
    protected override void JobSpecificSetup()
    {
        ticksBase = Mathf.FloorToInt(GenDate.TicksPerHour * 0.5f);
        ticksForEnhancer = GenDate.TicksPerHour * 1;
        context = LovinContext.Casual;

        // duration should eventually actually check for a love enhancer but rn just use base ticks
        duration = ticksBase;
    }
}