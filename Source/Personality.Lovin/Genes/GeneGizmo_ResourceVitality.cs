using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Personality.Lovin;

public class GeneGizmo_ResourceVitality : GeneGizmo_Resource
{
    public GeneGizmo_ResourceVitality(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barhighlightColor)
    : base(gene, drainGenes, barColor, barhighlightColor)
    {
        draggableBar = true;
    }

    protected override string GetTooltip()
    {
        string text = "testing";
        return text;
    }
}