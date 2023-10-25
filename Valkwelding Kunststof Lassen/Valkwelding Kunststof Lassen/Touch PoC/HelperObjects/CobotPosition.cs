using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.HelperObjects
{
    public class CobotPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Roll { get; set; }
        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public bool GeneratePointsBetweenLast {  get; set; }
    }
}
