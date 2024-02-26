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

    public static NeedDef PP_Need_Lovin;

    public static SkillDef LovinSkill;

    public static StatDef LovinQuality;

    public static ThoughtDef PP_ThoughtSocial_TerribleLovin;
    public static ThoughtDef PP_ThoughtSocial_BadLovin;
    public static ThoughtDef PP_ThoughtSocial_OkayLovin;
    public static ThoughtDef PP_ThoughtSocial_GoodLovin;
    public static ThoughtDef PP_ThoughtSocial_GreatLovin;
    public static ThoughtDef PP_ThoughtSocial_ExquisiteLovin;
    public static ThoughtDef PP_ThoughtSocial_TranscendentLovin;

    public static XenotypeDef PP_Succubus;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}