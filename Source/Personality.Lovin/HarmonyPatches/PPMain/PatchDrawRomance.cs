using HarmonyLib;
using UnityEngine;
using Verse;

namespace Personality.Lovin.HarmonyPatches;

[HarmonyPatch(typeof(MindCardUtility), nameof(MindCardUtility.DrawRomance))]
public static class PatchDrawRomance
{
    public static void Postfix(Rect rect, Pawn pawn)
    {
        GUI.MindCardUtility.DrawMindCardRomanceStuff(rect, pawn);
    }
}