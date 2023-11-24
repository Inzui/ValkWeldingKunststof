using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
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

        public CobotControllerService(IOptions<LocalConfig> configuration, ICobotConnectionService cobotConnect)
        {
            this._cob = cobotConnect;
            Speed = configuration.Value.CobotSettings.MovementSpeed;
        }

        public void MoveToDirect(CobotPosition destination)
        {
            CobotPosition currentPos = GetCobotPosition();
            destination.RoundValues();
            currentPos.RoundValues();

            float[] desPosArray = { destination.X, destination.Y, destination.Z, destination.Roll, destination.Pitch, destination.Yaw };

            _cob.sendCobotPos(desPosArray, Speed);

            while (!currentPos.EqualPosition(destination))
            {
                currentPos = GetCobotPosition();
                currentPos.RoundValues();
                //Debug.WriteLine($"Current: {currentPos} \nDest: {desPos}\n===========");
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

        private void PrintPoint(float[] point)
        {
            foreach (var p in point)
            {
                Debug.Write($"{p}, ");
            }
            Debug.WriteLine("");
        }

        public void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction)
        {
            CobotPosition nextPosition;
            if (direction == MovementDirection.Forward)
            {
                nextPosition = GetForwardMovementPosition(startingPosition);
            }
            else
            {
                nextPosition = GetBackwardMovementPosition(startingPosition);
            }

            MoveToDirect(nextPosition);
        }

        private CobotPosition GetForwardMovementPosition(CobotPosition startingPosition)
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

        private CobotPosition GetBackwardMovementPosition(CobotPosition startingPosition)
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
    }
}
