using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Personality.Lovin;

public static class RelationshipHelper
{
    public static readonly List<PawnRelationDef> romanticRelationDefs = new()
    {
        PawnRelationDefOf.Spouse,
        PawnRelationDefOf.Fiance,
        PawnRelationDefOf.Lover
    };

    public static bool IsPartnered(this Pawn pawn, out Pawn partner)
    {
        foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
        {
            if (romanticRelationDefs.Contains(rel.def))
            {
                partner = rel.otherPawn;
                return true;
            };
        }
        partner = null;
        return false;
    }

    public static bool IsLoverOf(this Pawn primary, Pawn maybePartner)
    {
        var relations = GetRelationsBetween(primary, maybePartner);
        foreach (var rel in relations)
        {
            if (romanticRelationDefs.Contains(rel.def)) return true;
        }
        return false;
    }

    public static IEnumerable<DirectPawnRelation> GetRelationsBetween(Pawn primary, Pawn otherPawn)
    {
        foreach (var rel in primary.relations.DirectRelations)
        {
            if (rel.otherPawn.ThingID == otherPawn.ThingID)
            {
                yield return rel;
            }
        }
    }
}