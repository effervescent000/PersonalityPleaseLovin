using Verse;

namespace Personality.Lovin;

public enum LovinContext
{
    SelfLovin,
    Casual,
    Intimate,
    Seduced,
}

public class LovinProps
{
    public Pawn Actor;
    public Pawn Partner;
    public LovinContext Context;

    public LovinProps(LovinContext context, Pawn actor, Pawn partner = null)
    {
        Actor = actor;
        Partner = partner;
        Context = context;
    }
}