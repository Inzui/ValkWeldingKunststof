using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class DetectionService : IDetectionService
    {
        private IDistanceDetector _distanceDetector;

        public DetectionService(IDistanceDetector distanceDetector)
        {
            _distanceDetector = distanceDetector;
        }

        public void Detect()
        {
            Debug.WriteLine(_distanceDetector.ObjectDetected);
        }
    }
}
