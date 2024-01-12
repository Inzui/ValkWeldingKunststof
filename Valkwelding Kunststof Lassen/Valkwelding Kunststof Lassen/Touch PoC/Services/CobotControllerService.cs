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

        /// <summary>
        /// Starts a milling sequence on a Cobot robot.
        /// </summary>
        /// <param name="cobotPositions">A collection of CobotPositions representing the positions to move to during the milling sequence.</param>
        /// <remarks>
        /// This method first sets the StepSize to _millingStepSize and starts the milling process. It then waits for one second before moving to each position in the provided collection at the speed defined by MillingSpeed. After all positions have been visited, it stops the milling process.
        /// </remarks>
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

        /// <summary>
        /// Moves the Cobot robot directly to a specified position at a certain speed.
        /// </summary>
        /// <param name="destination">The destination position for the robot.</param>
        /// <param name="speed">The speed at which the robot should move.</param>
        /// <remarks>
        /// This method first checks if the robot's current position is close enough to the destination (within half the step size). If it is, the method returns immediately.
        /// Otherwise, it sends a command to the robot to move to the destination at the specified speed. It then enters a loop where it continues to wait until the robot's current position is close enough to the destination.
        /// </remarks>
        public void MoveToDirect(CobotPosition destination, float speed)
        {
            float[] desPosArray = { destination.X, destination.Y, destination.Z, destination.Roll, destination.Pitch, destination.Yaw };

            if (CurrentPosition.EqualPosition(destination, StepSize / 2))
            {
                return;
            }

            _cob.SendCobotPos(desPosArray, speed);
            while (!CurrentPosition.EqualPosition(destination, StepSize / 2))
            {
                // Wait
            }
        }

        /// <summary>
        /// Gets the current position of the Cobot robot periodically.
        /// </summary>
        /// <param name="source">The object that raised the event.</param>
        /// <param name="e">Information about the elapsed event.</param>
        /// <remarks>
        /// This method runs every time the specified event is triggered. It checks if the robot is connected and if it's not currently getting its position.
        /// If these conditions are met, it sets _gettingPosition to true and attempts to get the robot's current position.
        /// If the robot is connected, it reads the position, locks the CurrentPosition object, updates its properties, and then releases the lock.
        /// If an exception occurs during this process, it writes the exception to the debug output.
        /// Finally, it sets _gettingPosition to false regardless of whether an exception occurred.
        /// </remarks>
        private async void GetCobotPositionEventAsync(object source, ElapsedEventArgs e)
        {
            if (_cob.CobotConnected && !_gettingPosition)
            {
                _gettingPosition = true;
                try
                {
                    await Task.Run(() =>
                    {
                        float[] currentPos = _cob.ReadPos();
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

        /// <summary>
        /// Moves the Cobot robot in steps towards an object.
        /// </summary>
        /// <param name="startingPosition">The starting position for the robot.</param>
        /// <param name="direction">The direction in which the robot should move.</param>
        /// <param name="noOfSteps">The number of steps the robot should take. Default is 1.</param>
        /// <remarks>
        /// This method moves the robot in a specified direction towards an object. It calculates the next position based on the starting position and the direction.
        /// If the direction is forward, it calls GetForwardMovementPosition, otherwise, it calls GetBackwardMovementPosition.
        /// Then, it moves the robot to the calculated position using the MoveToDirect method.
        /// This process repeats for the number of steps specified.
        /// </remarks>
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

        /// <summary>
        /// Calculates the new position for the Cobot robot to move forward.
        /// </summary>
        /// <param name="startingPosition">The starting position for the robot.</param>
        /// <returns>A CobotPosition representing the new position for the robot.</returns>
        /// <remarks>
        /// This method calculates the new position for the robot to move forward based on the starting position.
        /// It first calculates the angle alpha based on the yaw of the starting position and a predefined offset.
        /// Then, it calculates the deltas for x and y coordinates based on the cosine and sine of alpha multiplied by the step size.
        /// It creates a copy of the starting position and adds the calculated deltas to the x and y coordinates.
        /// Finally, it returns the updated position.
        /// </remarks>
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

        /// <summary>
        /// Calculates the new position for the Cobot robot to move backward.
        /// </summary>
        /// <param name="startingPosition">The starting position for the robot.</param>
        /// <returns>A CobotPosition representing the new position for the robot.</returns>
        /// <remarks>
        /// This method calculates the new position for the robot to move backward based on the starting position.
        /// It first calculates the angle alpha based on the yaw of the starting position and a predefined offset.
        /// Then, it calculates the deltas for x and y coordinates based on the cosine and sine of alpha multiplied by the step size.
        /// It creates a copy of the starting position and subtracts the calculated deltas from the x and y coordinates.
        /// Finally, it returns the updated position.
        /// </remarks>
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

        /// <summary>
        /// Adjusts the position of the Cobot robot by a milling offset.
        /// </summary>
        /// <param name="position">The current position of the robot.</param>
        /// <remarks>
        /// This method adjusts the position of the robot by calculating a milling offset based on the robot's yaw and a predefined offset.
        /// It then adds the calculated offsets to the x and y coordinates of the position.
        /// </remarks>
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
