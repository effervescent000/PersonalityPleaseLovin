using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Personality.Lovin;

public static class RelationshipHelper
{
    private static readonly List<PawnRelationDef> romanticRelationDefs = new()
    {
        PawnRelationDefOf.Spouse,
        PawnRelationDefOf.Fiance,
        PawnRelationDefOf.Lover
    };

    public static bool IsPartnered(this Pawn pawn)
    {
        foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
        {
            if (romanticRelationDefs.Contains(rel.def)) return true;
        }
        return false;
    }
}