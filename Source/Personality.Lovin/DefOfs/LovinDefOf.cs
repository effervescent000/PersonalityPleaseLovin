using RimWorld;
using Verse;

namespace Personality.Lovin;

[DefOf]
public static class LovinDefOf
{
    public static JobDef DoSelfLovin;
    public static JobDef PP_DoSeducedLovin;

    public static NeedDef PP_Need_Lovin;

    public static SkillDef LovinSkill;

    public static StatDef LovinQuality;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}