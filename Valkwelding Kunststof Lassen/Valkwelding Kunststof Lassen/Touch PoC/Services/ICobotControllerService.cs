using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        CobotPosition CurrentPosition { get; }
        float MovementSpeed { get; }
        float MillingSpeed { get; }
        float StepSize { get; set; }

        void Start();
        CobotPosition GetBackwardMovementPosition(CobotPosition startingPosition);
        CobotPosition GetForwardMovementPosition(CobotPosition startingPosition);
        void AddMillingOffsetPosition(CobotPosition position);
        void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction, int noOfSteps = 1);
        void MoveToDirect(CobotPosition destination, float speed);
        void StartMill();
        void StartMillSequence(IEnumerable<CobotPosition> cobotPositions);
        void StopMill();
    }
}