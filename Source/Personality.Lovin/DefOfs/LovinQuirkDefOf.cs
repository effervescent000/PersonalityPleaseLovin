using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinQuirkDefOf
{
    public static QuirkCategoryDef PP_RelationshipTypePreference;
    public static QuirkCategoryDef PP_Fidelity;

    public static QuirkDef PP_Monogamous;
    public static QuirkDef PP_Polyamorous;

    public static QuirkDef PP_Unwavering;
    public static QuirkDef PP_Faithful;
    public static QuirkDef PP_Unfaithful;
    public static QuirkDef PP_Cheater;

    static LovinQuirkDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinQuirkDefOf));
    }
}