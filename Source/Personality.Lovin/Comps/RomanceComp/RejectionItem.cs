using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class RejectionItem : IExposable
{
    public Pawn Pawn;
    public int TicksSinceAsked;

    public RejectionItem()
    {
    }

    public RejectionItem(Pawn pawn)
    {
        Pawn = pawn;
        TicksSinceAsked = 0;
    }

    public RejectionItem Get => this;

    public void ExposeData()
    {
        Scribe_References.Look(ref Pawn, "pawn");
        Scribe_Values.Look(ref TicksSinceAsked, "ticks");
    }
}