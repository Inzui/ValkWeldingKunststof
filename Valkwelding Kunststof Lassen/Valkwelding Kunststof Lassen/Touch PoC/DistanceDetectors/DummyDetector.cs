using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using ValkWelding.Welding.PolyTouchApplication.Configuration;
using ValkWelding.Welding.PolyTouchApplication.Types;

namespace ValkWelding.Welding.PolyTouchApplication.DistanceDetectors
{
    public class DummyDetector : IDistanceDetector
    {
        public bool Connected {  get; set; }

        public DummyDetector()
        {
            Connected = true;
        }

        public DetectorResponse SendCommand(DetectorCommand command)
        {
            throw new System.NotImplementedException();
        }

        public bool Connect(string comPort)
        {
            return true;
        }
    }
}
