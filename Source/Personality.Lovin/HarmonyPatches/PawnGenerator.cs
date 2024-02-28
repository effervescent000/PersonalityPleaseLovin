using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Personality.Lovin.HarmonyPatches;

[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new Type[] { typeof(PawnGenerationRequest) })]
public class PawnGenerator_Patches
{
    private static readonly List<string> vanillaSexualityTraits = new() { "Gay", "Bisexual", "Asexual" };

    [HarmonyPostfix]
    public static Pawn CleanAndApplySexualityTraits(Pawn pawn)
    {
        if (pawn.def.defName != "Human")
        {
            return pawn;
        }
        // this should no longer be necessary but I'm keeping it in for a bit to see if it pop stuff into the log
        //remove vanilla sexuality traits
        Trait sexualityTrait = null;
        foreach (Trait trait in pawn.story.traits.allTraits)
        {
            if (vanillaSexualityTraits.Contains(trait.def.defName))
            {
                sexualityTrait = trait;
            }
        }
        if (sexualityTrait is not null)
        {
            Log.Warning($"attempting to remove trait for pawn {pawn.Name}: {sexualityTrait.def.defName}");
            pawn.story.traits.RemoveTrait(sexualityTrait);

            // reroll a different trait
            Trait newTrait = null;
            while (newTrait is null)
            {
                List<Trait> rolledTrait = PawnGenerator.GenerateTraitsFor(pawn, 1);
                if (!vanillaSexualityTraits.Contains(rolledTrait[0].def.defName) && !pawn.story.traits.HasTrait(rolledTrait[0].def))
                {
                    newTrait = rolledTrait[0];
                }
            }
            pawn.story.traits.GainTrait(newTrait);
        }
        // now assign a sexuality trait
        SexualityHelper.RollSexualityTraitFor(pawn);

        return pawn;
    }
}