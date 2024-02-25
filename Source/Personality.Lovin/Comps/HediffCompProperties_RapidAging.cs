using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class HediffCompProperties_RapidAging : HediffCompProperties
{
    public float multiplierAtMax;

    public HediffCompProperties_RapidAging()
    {
        compClass = typeof(HediffComp_RapidAging);
    }
}