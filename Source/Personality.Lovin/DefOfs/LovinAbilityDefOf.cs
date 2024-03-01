using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinAbilityDefOf
{
    public static AbilityDef PP_SeduceAbility;
    public static AbilityDef PP_DisorientAbility;

    static LovinAbilityDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinAbilityDefOf));
    }
}