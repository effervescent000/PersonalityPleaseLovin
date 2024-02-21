using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinDefOf
{
    public static SkillDef LovinSkill;

    static LovinDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinDefOf));
    }
}