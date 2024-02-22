using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personality.Lovin.HarmonyPatches;

public static class PPR
{
    public static void Patch(Harmony harmony)
    {
        Type lovinHelper = AccessTools.TypeByName("Personality.Romance.LovinHelper");

        harmony.Patch(AccessTools.Method(lovinHelper, "TryDoSelfLovin"), postfix: new HarmonyMethod(AccessTools.Method(typeof(PatchTryDoSelfLovin), nameof(PatchTryDoSelfLovin.Postfix))));
        harmony.Patch(AccessTools.Method(lovinHelper, "EvaluateLovin"), postfix: new HarmonyMethod(AccessTools.Method(typeof(PatchEvaluateLovin), nameof(PatchEvaluateLovin.Postfix))));
    }
}