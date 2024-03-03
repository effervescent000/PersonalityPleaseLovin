using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private static readonly float SPOUSE_IMPORTANCE = PawnRelationDefOf.Spouse.importance;
    private static readonly float FIANCE_IMPORTANCE = PawnRelationDefOf.Fiance.importance;
    private static readonly float LOVER_IMPORTANCE = PawnRelationDefOf.Lover.importance;

    public static bool IsPartnered(this Pawn pawn, out List<Pawn> partners)
    {
        partners = new();
        foreach (DirectPawnRelation rel in pawn.relations.DirectRelations)
        {
            if (romanticRelationDefs.Contains(rel.def))
            {
                partners.Add(rel.otherPawn);
            };
        }

        return partners.Count > 0;
    }

    public static bool IsLoverOf(this Pawn primary, Pawn maybePartner)
    {
        IEnumerable<DirectPawnRelation> relations = GetRelationsBetween(primary, maybePartner);
        foreach (DirectPawnRelation rel in relations)
        {
            if (romanticRelationDefs.Contains(rel.def)) return true;
        }
        return false;
    }

    public static IEnumerable<DirectPawnRelation> GetRelationsBetween(Pawn primary, Pawn otherPawn)
    {
        foreach (DirectPawnRelation rel in primary.relations.DirectRelations)
        {
            if (rel.otherPawn.ThingID == otherPawn.ThingID)
            {
                yield return rel;
            }
        }
    }

    public static List<DirectPawnRelation> GetRelationsBetweenSorted(Pawn primary, Pawn otherPawn)
    {
        return GetRelationsBetween(primary, otherPawn).OrderByDescending((rel) => rel.def.importance).ToList();
    }

    /// <summary>
    /// should be used in pawn decision-making about who to sleep with, NOT for the purpose of
    /// assigning thoughts or whatever (at least for now)
    /// </summary>
    /// <param name="primary"></param>
    /// <param name="maybePartner"></param>
    /// <returns></returns>
    public static bool WouldBeCheating(Pawn primary, Pawn maybePartner)
    {
        MindComp primaryMind = primary.GetComp<MindComp>();
        Quirk relType = primaryMind?.GetQuirksByCategory(LovinQuirkDefOf.PP_RelationshipTypePreference).First();
        if (relType == null && relType.Def != LovinQuirkDefOf.PP_Monogamous) return false;

        if (primary.IsPartnered(out List<Pawn> existingPartners))
        {
            if (!existingPartners.Contains(maybePartner))
            {
                return true;
            }
            if (existingPartners.Count == 1) return false;

            // if a pawn is monogamous and has multiple partners, sort them by relationship type. so
            // for the purposes of this function, pawn A sleeping with their lover while A is
            // married counts as cheating, but pawn A sleeping with their spouse does not count as
            // cheating on their lover. for now, we're not thinking about polygamous ideos or
            // whatever, so if pawn A has a spouse and sleeps with another pawn, it's always
            // cheating, even if they're also married to said other pawn.

            List<Pair<Pawn, float>> relImportanceList = new();

            foreach (Pawn partner in existingPartners)
            {
                List<DirectPawnRelation> relations = GetRelationsBetweenSorted(primary, partner);
                relImportanceList.Add(new(partner, relations[0].def.importance));
            }

            relImportanceList.SortByDescending((pair) => pair.Second);

            DirectPawnRelation maybePartnerRelation = GetRelationsBetweenSorted(primary, maybePartner)[0];

            // do a for loop here solely so we can account for a situation where the pawn we're
            // checking for happens to be first in the list
            for (int i = 0; i < Math.Min(relImportanceList.Count, 2); i++)
            {
                if (relImportanceList[i].First == maybePartner) continue;

                if (relImportanceList[i].Second >= maybePartnerRelation.def.importance)
                {
                    return true;
                }
            }
        }
        return false;
    }
}