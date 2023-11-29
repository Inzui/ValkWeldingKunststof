using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IPathPlanningService
    {
        IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints, int amountOfPoints);
        void ReturnToStartPos(IEnumerable<CobotPosition> cobotPositions);
        void Start();
    }
}