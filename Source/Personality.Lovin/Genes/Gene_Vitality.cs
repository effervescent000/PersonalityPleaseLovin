using RimWorld;
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

    protected override Color BarColor => new(.96f, .62f, .79f);

    protected override Color BarHighlightColor { get; }

    public override void Tick()
    {
        base.Tick();
        GeneResourceDrainUtility.TickResourceDrain(this);
    }
}