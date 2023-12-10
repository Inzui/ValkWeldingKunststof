using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class CobotControllerService : ICobotControllerService
    {
        private ICobotConnectionService _cob;
        public float Speed { get; set; }
        public float StepSize { get; set; }

        private float _millingStepSize;

        public CobotControllerService(IOptions<LocalConfig> configuration, ICobotConnectionService cobotConnect)
        {
            _cob = cobotConnect;
            Speed = configuration.Value.CobotSettings.MovementSpeed;
            _millingStepSize = configuration.Value.CobotSettings.MovementPreciseStepSize;
        }

        public void StartMillSequence(IEnumerable<CobotPosition> cobotPositions)
        {
            StepSize = _millingStepSize;
            StartMill();
            Thread.Sleep(1000);
            foreach (CobotPosition cobotPosition in cobotPositions)
            {
                MoveToDirect(cobotPosition);
            }
            StopMill();
        }

        public void MoveToDirect(CobotPosition destination)
        {
            CobotPosition currentPos = GetCobotPosition();
            float[] desPosArray = { destination.X, destination.Y, destination.Z, destination.Roll, destination.Pitch, destination.Yaw };

            if (currentPos.EqualPosition(destination, StepSize / 2))
            {
                Thread.Sleep(500);
                return;
            }

            _cob.sendCobotPos(desPosArray, Speed);
            while (!currentPos.EqualPosition(destination, StepSize / 2))
            {
                currentPos = GetCobotPosition();
            }
        }

        public CobotPosition GetCobotPosition()
        {
            float[] currentPos = _cob.readPos();

            CobotPosition cobotPos = new()
            {
                X = currentPos[0],
                Y = currentPos[1],
                Z = currentPos[2],
                Roll = currentPos[3] + 180,
                Pitch = (-currentPos[4]) + 180,
                Yaw = currentPos[5] + 180
            };

            return cobotPos;
        }

        public void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction, int noOfSteps = 1)
        {
            CobotPosition nextPosition;
            for(int i = 0; i < noOfSteps; i++)
            {
                if (direction == MovementDirection.Forward)
                {
                    nextPosition = GetForwardMovementPosition(startingPosition);
                }
                else
                {
                    nextPosition = GetBackwardMovementPosition(startingPosition);
                }

                MoveToDirect(nextPosition);
                startingPosition = nextPosition;
            }
        }

        public CobotPosition GetForwardMovementPosition(CobotPosition startingPosition)
        {
            //Place head in right direction
            float alpha = (float)((-startingPosition.Yaw + 90) * Math.PI / 180.0);

            float deltaX = (float)Math.Cos(alpha) * StepSize;
            float deltaY = (float)Math.Sin(alpha) * -StepSize;

            CobotPosition newPosition = startingPosition.Copy();
            newPosition.X += deltaX;
            newPosition.Y += deltaY;

            return newPosition;
        }

        public CobotPosition GetBackwardMovementPosition(CobotPosition startingPosition)
        {
            //Place head in right direction
            float alpha = (float)((-startingPosition.Yaw + 90) * Math.PI / 180.0);

            float deltaX = (float)Math.Cos(alpha) * StepSize;
            float deltaY = (float)Math.Sin(alpha) * -StepSize;

            CobotPosition newPosition = startingPosition.Copy();
            newPosition.X -= deltaX;
            newPosition.Y -= deltaY;

            return newPosition;
        }

        public void StartMill()
        {
            _cob.SetCoilValue(7206, true);
        }

        public void StopMill()
        {
            _cob.SetCoilValue(7206, false);
        }
    }
}
