using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public abstract class Preference
{
    public float Value;

    public abstract string Label { get; }

    public abstract float CalcAttractionEffect(Pawn pawn);
}