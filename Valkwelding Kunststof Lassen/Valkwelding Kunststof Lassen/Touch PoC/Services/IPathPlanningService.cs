using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IPathPlanningService
    {
        IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints);
        void ReturnToStartPos(IEnumerable<CobotPosition> cobotPositions, bool generatePointsBetween);
        void Start();
        List<CobotPosition> UpdatePointsMilling(IEnumerable<CobotPosition> cobotPositions);
    }
}