using RimWorld;
using Verse;

namespace Personality.Lovin;

[DefOf]
public static class LovinDefOf
{
    public static JobDef DoSelfLovin;

    public static SkillDef LovinSkill;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}