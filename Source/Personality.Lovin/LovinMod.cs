using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

internal class LovinMod : Mod
{
    public static Settings settings;

    public LovinMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
    }
}