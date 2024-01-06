using System.Collections.Generic;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;

namespace ValkWelding.Welding.PolyTouchApplication.Services
{
    public interface IPathPlanningService
    {
        IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints);
        void ReturnToStartPos(IEnumerable<CobotPosition> cobotPositions, bool generatePointsBetween);
        void Start();
        List<CobotPosition> UpdatePointsMilling(IEnumerable<CobotPosition> cobotPositions);
    }
}