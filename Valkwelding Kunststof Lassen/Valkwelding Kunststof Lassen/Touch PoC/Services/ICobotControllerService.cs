using System.Collections.Generic;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;
using ValkWelding.Welding.PolyTouchApplication.Types;

namespace ValkWelding.Welding.PolyTouchApplication.Services
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