using RimWorld;
using Verse;

namespace Personality.Lovin;

[DefOf]
public static class LovinDefOf
{
    public static GeneDef PP_LoveFeeder;
    public static GeneDef PP_VitalityRoot;

    public static HediffDef PP_VitalityLost;

    public static JobDef DoSelfLovin;
    public static JobDef PP_DoSeducedLovin;
    public static JobDef DoCasualLovin;
    public static JobDef LeadHookup;

    public static JobDef PP_InitiateIntimateLovin;
    public static JobDef PP_DoIntimateLovin;

    public static NeedDef PP_Need_Lovin;

    public static SkillDef LovinSkill;

    public static StatDef LovinQuality;

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

    public static TraitDef Straight;
    public static TraitDef AroAce;
    public static TraitDef AceHetero;
    public static TraitDef AceBi;
    public static TraitDef AceHomo;

    public static XenotypeDef PP_Succubus;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}