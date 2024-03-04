using RimWorld;
using Verse;

namespace Personality.Lovin;

[DefOf]
public static class LovinJobDefOf
{
    public static JobDef DoSelfLovin;

    public static JobDef PP_DoSeducedLovin;
    public static JobDef PP_DoSeducedLovinLead;

    public static JobDef LeadHookup;
    public static JobDef DoCasualLovin;
    public static JobDef PP_DoCasualLovinLead;

    public static JobDef PP_InitiateIntimateLovin;
    public static JobDef PP_DoIntimateLovin;
    public static JobDef PP_DoIntimateLovinLead;

    static LovinJobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinJobDefOf));
    }
}