using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.Types
{
    public enum DetectorCommand : byte
    {
        Unknown = 0x0,
        StartDetecting = 0x01,
        EnableProbe = 0x02,
        Synced = 0x04
    }
}
