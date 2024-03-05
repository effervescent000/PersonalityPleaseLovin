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
    public int Duration;

    public LovinProps(LovinContext context, Pawn actor, Pawn partner = null, bool isInitiator = false, int duration = 0)
    {
        Actor = actor;
        Partner = partner;
        Context = context;
        IsInitiator = isInitiator;
        Duration = duration;
    }
}