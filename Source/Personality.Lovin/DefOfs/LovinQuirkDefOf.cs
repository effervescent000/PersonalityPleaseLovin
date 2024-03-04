using RimWorld;

namespace Personality.Lovin;

[DefOf]
public static class LovinQuirkDefOf
{
    public static QuirkCategoryDef PP_RelationshipTypePreference;
    public static QuirkCategoryDef PP_Fidelity;

    //public static QuirkDef PP_Fidelity;
    //public static QuirkDef PP_RomanceSeeking;

    public static QuirkDef PP_Monogamous;
    public static QuirkDef PP_Polyamorous;

    static LovinQuirkDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(LovinQuirkDefOf));
    }
}