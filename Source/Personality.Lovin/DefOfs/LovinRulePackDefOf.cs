using RimWorld;
using Verse;

namespace Personality.Lovin;

[DefOf]
public static class LovinRulePackDefOf
{
    public static InteractionDef PP_TriedHookup;
    public static InteractionDef PP_TriedIntimacy;
    public static InteractionDef PP_SeduceInteraction;

    public static RulePackDef PP_HookupSucceeded;
    public static RulePackDef PP_HookupFailed;
    public static RulePackDef PP_IntimacyFailed;
    public static RulePackDef PP_IntimacySucceeded;
    public static RulePackDef PP_SeductionSucceeded;
    public static RulePackDef PP_SeductionFailed;

    static LovinRulePackDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinRulePackDefOf));
    }
}