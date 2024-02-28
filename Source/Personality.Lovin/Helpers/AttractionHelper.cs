using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Personality.Lovin;

public static class AttractionHelper
{
    public static List<string> HairStyleTags = new() { "Urban", "Rural", "Wild", "Punk", "Tribal", "Soldier", "HairLong", "HairShort", "Balding", "Bald" };

    public static List<BodyTypeDef> GenericBodyTypes = new() { BodyTypeDefOf.Hulk, BodyTypeDefOf.Fat, BodyTypeDefOf.Thin };

    // scrape hair color list off of genes. does this make biotech a hard requirement or are hair
    // genes in the basegame now?
    public static List<GeneDef> HairColorGenes = (from c in DefDatabase<GeneDef>.AllDefsListForReading
                                                  where c.endogeneCategory == EndogeneCategory.HairColor
                                                  select c).ToList();

    public static List<HeadTypeDef> MaleHeads = (from head in DefDatabase<HeadTypeDef>.AllDefsListForReading
                                                 where head.gender == Gender.Male
                                                 select head).ToList();

    public static List<HeadTypeDef> FemaleHeads = (from head in DefDatabase<HeadTypeDef>.AllDefsListForReading
                                                   where head.gender == Gender.Female
                                                   select head).ToList();
}