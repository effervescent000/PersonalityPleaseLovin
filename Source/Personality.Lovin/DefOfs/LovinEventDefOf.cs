using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinEventDefOf
{
    public static HistoryEventDef PP_GotLovinSuccubus;
    public static HistoryEventDef PP_GotLovinSeduced;
    public static HistoryEventDef PP_SeductionWilling;
    public static HistoryEventDef PP_SeductionResistFailed;
    public static HistoryEventDef PP_SeductionResisted;

    static LovinEventDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinEventDefOf));
    }
}