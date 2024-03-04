using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Personality.Lovin;

public class JobDriver_DoLovinSeducedLead : JobDriver_DoLovinSeduced
{
    protected override void SetInitiator()
    {
        isInitiator = true;
    }
}