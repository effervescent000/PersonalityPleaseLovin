using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Personality.Lovin;

public class Gene_Vitality : Gene_Resource, IGeneResourceDrain
{
    public Gene_Resource Resource => this;

    public bool CanOffset => pawn.Spawned && Active;

    public float ResourceLossPerDay => def.resourceLossPerDay;

    public Pawn Pawn => pawn;

    public string DisplayLabel => def.resourceLabel;

    public override float InitialResourceMax => 1f;

    public override float MinLevelForAlert => 0.25f;

    protected override Color BarColor { get; }

    protected override Color BarHighlightColor { get; }
}