using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IDetectionService
    {
        void Detect(IEnumerable<CobotPosition> measurePoints, int pointsBetween);
    }
}