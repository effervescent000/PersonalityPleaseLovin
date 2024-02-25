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