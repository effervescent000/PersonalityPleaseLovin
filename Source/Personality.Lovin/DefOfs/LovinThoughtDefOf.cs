using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinThoughtDefOf
{
    public static ThoughtDef PP_TurnedMeDownForHookup;
    public static ThoughtDef PP_TurnedMeDownForHookup_Mood;
    public static ThoughtDef PP_HadToRejectSomeoneForHookup;

    public static ThoughtDef PP_TurnedMeDownForIntimacy;
    public static ThoughtDef PP_TurnedMeDownForIntimacy_Mood;
    public static ThoughtDef PP_HadToRejectSomeoneForIntimacy;

    public static ThoughtDef PP_ThoughtSocial_TerribleLovin;
    public static ThoughtDef PP_ThoughtSocial_BadLovin;
    public static ThoughtDef PP_ThoughtSocial_OkayLovin;
    public static ThoughtDef PP_ThoughtSocial_GoodLovin;
    public static ThoughtDef PP_ThoughtSocial_GreatLovin;
    public static ThoughtDef PP_ThoughtSocial_ExquisiteLovin;
    public static ThoughtDef PP_ThoughtSocial_TranscendentLovin;

    public static ThoughtDef PP_CheatedGuilty;
    public static ThoughtDef PP_CheatedDontCare;
    public static ThoughtDef PP_CheatedHappy;

    static LovinThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinThoughtDefOf));
    }
}