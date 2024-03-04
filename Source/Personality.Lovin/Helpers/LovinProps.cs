using Verse;

namespace Personality.Lovin;

public enum LovinContext
{
    SelfLovin,
    Casual,
    Intimate,
    Seduced,
}

public struct LovinProps
{
    public Pawn Actor;
    public Pawn Partner;
    public LovinContext Context;
    public bool IsInitiator;

    public LovinProps(LovinContext context, Pawn actor, Pawn partner = null, bool isInitiator = false)
    {
        Actor = actor;
        Partner = partner;
        Context = context;
        IsInitiator = isInitiator;
    }
}