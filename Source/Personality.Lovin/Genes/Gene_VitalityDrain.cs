using RimWorld;
using Verse;

namespace Personality.Lovin;

public class Gene_VitalityDrain : Gene, IGeneResourceDrain
{
    private Gene_Vitality cachedVitalityGene;

    public Gene_Resource Resource
    {
        get
        {
            if (cachedVitalityGene == null || !cachedVitalityGene.Active)
            {
                cachedVitalityGene = pawn.genes.GetFirstGeneOfType<Gene_Vitality>();
            }
            return cachedVitalityGene;
        }
    }

    public bool CanOffset => pawn.Spawned && Active;

    public float ResourceLossPerDay => def.resourceLossPerDay;

    public Pawn Pawn => pawn;

    public string DisplayLabel => def.label;

    public override void Tick()
    {
        base.Tick();
        GeneResourceDrainUtility.TickResourceDrain(this);
    }
}