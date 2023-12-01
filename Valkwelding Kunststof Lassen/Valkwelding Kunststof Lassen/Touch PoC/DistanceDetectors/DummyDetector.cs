using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.DistanceDetectors
{
    public class DummyDetector : IDistanceDetector
    {
        public bool Connected { get; }
        public bool ObjectDetected { get; set; }

        public void Start() { }
        public void EnableProbe() { }
    }
}
