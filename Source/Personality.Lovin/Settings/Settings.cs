using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Personality.Lovin;

public class Settings : ModSettings
{
    public static bool RomanceModuleActive = false;

    public override void ExposeData()
    {
        base.ExposeData();
    }
}