using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Personality.Lovin.HarmonyPatches;

public static class PatchTryDoSelfLovin
{
    public static Job Postfix(Job _, Pawn pawn)
    {
        return LovinHelper.TryDoSelfLovin(pawn);
    }
}