using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.PolyTouchApplication.Configuration
{
    public class CobotSettings
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public float YawOffsetDegrees { get; set; }
        public float MovementSpeed { get; set; }
        public float MillingMovementSpeed { get; set; }
        public float MovementRoughStepSize { get; set; }
        public float MovementPreciseStepSize { get; set; }
        public float MillOffset { get; set; }
    }
}
