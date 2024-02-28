using Verse;

namespace Personality.Lovin;

public class AttractionEvaluation
{
    public Pawn Target;
    public float PhysicalScore = 1f;
    public float PersonalityScore = 1f;
    public int TicksSinceCache = 0;

    public AttractionEvaluation(Pawn target)
    {
        Target = target;
    }

    public void MakeEval(AttractionTracker attraction)
    {
        TicksSinceCache = 0;
        foreach (Preference pref in attraction.AllPrefs)
        {
            PhysicalScore += pref.CalcAttractionEffect(Target);
        }
        PersonalityScore = attraction.pawn.relations.CompatibilityWith(Target);
    }

    public void Tick()
    {
        TicksSinceCache++;
    }
}