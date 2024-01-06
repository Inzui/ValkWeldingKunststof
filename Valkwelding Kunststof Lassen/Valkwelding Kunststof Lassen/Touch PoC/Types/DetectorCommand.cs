using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.PolyTouchApplication.Types
{
    public enum DetectorCommand : byte
    {
        Unknown = 0x0,
        Heartbeat = 0x01,
        StartDetecting = 0x02,
        RequestObjectDetected = 0x04
    }
}
