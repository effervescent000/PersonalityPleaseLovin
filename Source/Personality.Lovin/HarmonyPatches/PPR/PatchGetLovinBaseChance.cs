using Verse;

namespace Personality.Lovin.HarmonyPatches;

public static class PatchGetLovinBaseChance
{
    public static float Postfix(float _, Pawn pawn)
    {
        return LovinHelper.GetChanceToSeekLovin(pawn);
    }
}