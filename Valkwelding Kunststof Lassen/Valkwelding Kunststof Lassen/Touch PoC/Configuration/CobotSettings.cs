using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.Configuration
{
    public class CobotSettings
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public float MovementSpeed { get; set; }
        public float MovementStepSize { get; set; }
    }
}
