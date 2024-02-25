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

    public static XenotypeDef PP_Succubus;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}