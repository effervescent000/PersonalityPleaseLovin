using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class RomanceTracker : IExposable
{
    private HashSet<RejectionItem> rejectionList = new();

    public RomanceTracker()
    {
    }

    public HashSet<RejectionItem> RejectionList => rejectionList;

    public bool IsInRejectionList(Pawn target)
    {
        foreach (var item in rejectionList)
        {
            if (item.Pawn.ThingID == target.ThingID) return true;
        }
        return false;
    }

    public void Tick()
    {
        List<RejectionItem> itemsToRemove = new();

        foreach (RejectionItem item in rejectionList)
        {
            item.TicksSinceAsked++;
            if (item.TicksSinceAsked > GenDate.TicksPerDay * 2)
            {
                itemsToRemove.Add(item);
            }
        }
        foreach (RejectionItem item in itemsToRemove)
        {
            rejectionList.Remove(item);
        }
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref rejectionList, "rejections", LookMode.Deep);
    }
}