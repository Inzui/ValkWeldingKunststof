using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ValkWelding.Welding.PolyTouchApplication.Configuration;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;
using ValkWelding.Welding.PolyTouchApplication.Types;

namespace ValkWelding.Welding.PolyTouchApplication.Services
{
    public class CobotControllerService : ICobotControllerService
    {
        private readonly ICobotConnectionService _cob;
        public float MovementSpeed { get; private set; }
        public float MillingSpeed { get; private set; }
        public float StepSize { get; set; }
        public CobotPosition CurrentPosition { get; private set; }

        private readonly float _yawOffsetDegrees;
        private readonly float _millingStepSize;
        private readonly float _millingOffset;

        private readonly System.Timers.Timer _gettingPositionTimer;
        private bool _gettingPosition;

        public CobotControllerService(IOptions<LocalConfig> configuration, ICobotConnectionService cobotConnect)
        {
            _cob = cobotConnect;
            MovementSpeed = configuration.Value.CobotSettings.MovementSpeed;
            MillingSpeed = configuration.Value.CobotSettings.MillingMovementSpeed;
            _yawOffsetDegrees = configuration.Value.CobotSettings.YawOffsetDegrees;
            _millingStepSize = configuration.Value.CobotSettings.MovementPreciseStepSize;
            _millingOffset = configuration.Value.CobotSettings.MillOffset;
            CurrentPosition = new();

            _gettingPositionTimer = new();
            _gettingPositionTimer.Interval = 10;
            _gettingPositionTimer.Elapsed += new ElapsedEventHandler(GetCobotPositionEventAsync);
            _gettingPosition = false;
        }

        public void Start()
        {
            _gettingPositionTimer.Start();
        }

        public void StartMillSequence(IEnumerable<CobotPosition> cobotPositions)
        {
            StepSize = _millingStepSize;
            StartMill();
            Thread.Sleep(1000);

            foreach (CobotPosition cobotPosition in cobotPositions)
            {
                MoveToDirect(cobotPosition, MillingSpeed);
            }

            StopMill();
        }

        public void MoveToDirect(CobotPosition destination, float speed)
        {
            float[] desPosArray = { destination.X, destination.Y, destination.Z, destination.Roll, destination.Pitch, destination.Yaw };

            if (CurrentPosition.EqualPosition(destination, StepSize / 2))
            {
                return;
            }

            _cob.sendCobotPos(desPosArray, speed);
            while (!CurrentPosition.EqualPosition(destination, StepSize / 2))
            {
                // Wait
            }
        }

        private async void GetCobotPositionEventAsync(object source, ElapsedEventArgs e)
        {
            if (_cob.CobotConnected && !_gettingPosition)
            {
                _gettingPosition = true;
                try
                {
                    await Task.Run(() =>
                    {
                        float[] currentPos = _cob.readPos();
                        lock (CurrentPosition)
                        {
                            CurrentPosition = new()
                            {
                                X = currentPos[0],
                                Y = currentPos[1],
                                Z = currentPos[2],
                                Roll = currentPos[3] + 180,
                                Pitch = (-currentPos[4]) + 180,
                                Yaw = currentPos[5] + 180
                            };
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    _gettingPosition = false;
                }
            }
        }

        public void MoveStepToObject(CobotPosition startingPosition, MovementDirection direction, int noOfSteps = 1)
        {
            CobotPosition nextPosition;
            for (int i = 0; i < noOfSteps; i++)
            {
                if (direction == MovementDirection.Forward)
                {
                    nextPosition = GetForwardMovementPosition(startingPosition);
                }
                else
                {
                    nextPosition = GetBackwardMovementPosition(startingPosition);
                }

                MoveToDirect(nextPosition, MillingSpeed);
                startingPosition = nextPosition;
            }
        }

        public CobotPosition GetForwardMovementPosition(CobotPosition startingPosition)
        {
            //Place head in right direction
            float alpha = (float)((-startingPosition.Yaw + _yawOffsetDegrees) * Math.PI / 180.0);

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
            float alpha = (float)((-startingPosition.Yaw + _yawOffsetDegrees) * Math.PI / 180.0);

            float deltaX = (float)Math.Cos(alpha) * StepSize;
            float deltaY = (float)Math.Sin(alpha) * -StepSize;

            CobotPosition newPosition = startingPosition.Copy();
            newPosition.X -= deltaX;
            newPosition.Y -= deltaY;

            return newPosition;
        }

        public void AddMillingOffsetPosition(CobotPosition position)
        {
            //Place head in right direction
            float alpha = (float)((-position.Yaw + _yawOffsetDegrees) * Math.PI / 180.0);

            float deltaX = (float)Math.Cos(alpha) * _millingOffset;
            float deltaY = (float)Math.Sin(alpha) * -_millingOffset;

            position.X += deltaX;
            position.Y += deltaY;
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
