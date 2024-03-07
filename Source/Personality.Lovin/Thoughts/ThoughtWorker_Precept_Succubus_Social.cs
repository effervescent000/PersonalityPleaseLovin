using RimWorld;
using Verse;

namespace Personality.Lovin;

public class ThoughtWorker_Precept_Succubus_Social : ThoughtWorker_Precept_Social
{
    protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.IsLoveFeeder();
    }
}