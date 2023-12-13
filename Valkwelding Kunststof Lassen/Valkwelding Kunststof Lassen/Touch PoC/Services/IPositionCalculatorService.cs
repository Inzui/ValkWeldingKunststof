using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface IPositionCalculatorService
    {
        CobotPosition GetCornerPosition(CobotPosition positionOne, CobotPosition positionTwo);
        CobotPosition GetCornerPosition(CobotPosition[] positionsOne, CobotPosition[] positionsTwo);
        List<CobotPosition> GetPointsOnCircle(CobotPosition posOne, CobotPosition posTwo, CobotPosition posThree, int noOfPoints);
    }
}