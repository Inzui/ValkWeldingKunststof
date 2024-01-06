using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.PolyTouchApplication.Types
{
    public enum DetectorResponse
    {
        UNKNOWN = 0x0,
        Succes = 0x1,
        ObjectDetected = 0x2,
        ObjectNotDetected = 0x4
    }
}
