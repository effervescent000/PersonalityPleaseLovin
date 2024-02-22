using Personality.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace Personality.Lovin.HarmonyPatches;

public static class PatchEvaluateLovin
{
    public static Toil Postfix(Toil _, LovinProps props)
    {
        return new Toil
        {
            initAction = delegate
            {
                LovinHelper.EvaluateLovin(props);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}