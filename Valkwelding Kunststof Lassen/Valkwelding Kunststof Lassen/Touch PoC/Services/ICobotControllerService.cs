﻿using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public interface ICobotControllerService
    {
        float Speed { get; set; }
        float StepSize { get; set; }

        CobotPosition GetBackwardMovementPosition(CobotPosition startingPosition);
        CobotPosition GetCobotPosition();
        CobotPosition GetForwardMovementPosition(CobotPosition startingPosition);
        void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction);
        void MoveToDirect(CobotPosition destination);
        void StartMill();
        void StartMillSequence(IEnumerable<CobotPosition> cobotPositions);
        void StopMill();
    }
}