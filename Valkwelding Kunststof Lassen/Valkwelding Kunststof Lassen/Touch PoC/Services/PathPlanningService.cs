using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ValkWelding.Welding.PolyTouchApplication.Configuration;
using ValkWelding.Welding.PolyTouchApplication.DistanceDetectors;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;
using ValkWelding.Welding.PolyTouchApplication.Types;
using ValkWelding.Welding.PolyTouchApplication.ViewModels;

namespace ValkWelding.Welding.PolyTouchApplication.Services
{
    public class PathPlanningService : IPathPlanningService
    {
        private readonly ICobotConnectionService _cobotConnectionService;
        private readonly ICobotControllerService _cobotController;
        private readonly PositionCalculatorService _positionCalculatorService;
        private readonly IDistanceDetector _distanceDetector;
        private readonly SettingsViewModel _settingsViewModel;

        private System.Timers.Timer _getPositionTimer;
        private bool _busy;

        private readonly float _roughStepSize;
        private readonly float _preciseStepSize;

        public PathPlanningService(IOptions<LocalConfig> configuration, ICobotConnectionService connectionService, ICobotControllerService cobotController, IDistanceDetector distanceDetector, PositionCalculatorService positionCalculator, SettingsViewModel settingsViewModel)
        {
            _cobotConnectionService = connectionService;
            _cobotController = cobotController;
            _distanceDetector = distanceDetector;
            _settingsViewModel = settingsViewModel;
            _positionCalculatorService = positionCalculator;

            _getPositionTimer = new();
            _getPositionTimer.Elapsed += new ElapsedEventHandler(GetCurrentCobotPositionEvent);
            _getPositionTimer.Interval = 500;
            _busy = false;

            _roughStepSize = configuration.Value.CobotSettings.MovementRoughStepSize;
            _preciseStepSize = configuration.Value.CobotSettings.MovementPreciseStepSize;
        }

        public void Start()
        {
            _getPositionTimer.Enabled = true;
        }

        /// <summary>
        /// Updates the current position of the Cobot robot periodically.
        /// </summary>
        /// <param name="source">The object that raised the event.</param>
        /// <param name="e">Information about the elapsed event.</param>
        /// <remarks>
        /// This method runs every time the specified event is triggered. It checks if the robot is not currently busy.
        /// If the robot is connected, it attempts to update the current position of the robot.
        /// If an exception occurs during this process, it sets the CobotConnected flag to false, displays a connection error message, and writes the exception to the debug output.
        /// Finally, it sets _busy to false regardless of whether an exception occurred.
        /// </remarks>
        private void GetCurrentCobotPositionEvent(object source, ElapsedEventArgs e)
        {
            if (!_busy)
            {
                _busy = true;
                if (_cobotConnectionService.CobotConnected)
                {
                    try
                    {
                        _settingsViewModel.CurrentCobotPosition = _cobotController.CurrentPosition;
                    }
                    catch (Exception ex)
                    {
                        _cobotConnectionService.CobotConnected = false;
                        _settingsViewModel.MessageBoxText = "Connection Error";
                        Debug.WriteLine(ex);
                    }
                }
                _busy = false;
            }
        }

        /// <summary>
        /// Returns the Cobot robot to its start position.
        /// </summary>
        /// <param name="cobotPositions">A collection of CobotPositions representing the positions to move to.</param>
        /// <param name="generatePointsBetween">A boolean indicating whether to generate intermediate points between the positions.</param>
        /// <remarks>
        /// This method first sets the StepSize of the CobotController to _roughStepSize.
        /// It then checks if it should generate points between the positions. If so, it generates these points.
        /// It reverses the order of the positions and moves the robot to each position in reverse order.
        /// Finally, it doubles the StepSize, moves the robot back to the original start position with a backwards offset, and resets the StepSize to _roughStepSize.
        /// </remarks>
        public void ReturnToStartPos(IEnumerable<CobotPosition> cobotPositions, bool generatePointsBetween)
        {
            _cobotController.StepSize = _roughStepSize;

            IEnumerable<CobotPosition> fullCobotPositions = cobotPositions;
            if (generatePointsBetween)
            {
                fullCobotPositions = GeneratePointsBetween(cobotPositions);
            }
            
            IEnumerable<CobotPosition> reversedPositions = fullCobotPositions.Reverse();

            foreach (CobotPosition cobotPosition in reversedPositions)
            {
                _cobotController.MoveToDirect(cobotPosition, _cobotController.MovementSpeed);
            }
            _cobotController.StepSize = _roughStepSize*2;
            _cobotController.MoveToDirect(_cobotController.GetBackwardMovementPosition(reversedPositions.Last()), _cobotController.MovementSpeed);
        }

        /// <summary>
        /// Detects objects at the given measure points and adjusts their positions.
        /// </summary>
        /// <param name="measurePoints">A collection of CobotPositions representing the points to detect objects at.</param>
        /// <returns>A collection of CobotPositions representing the detected objects' positions.</returns>
        /// <remarks>
        /// This method first generates points between the measure points.
        /// For each generated position, it sets the StepSize of the CobotController to _roughStepSize and moves the robot to the position.
        /// If the position type is not dummy, it sends commands to the distance detector to start detecting and request if an object has been detected.
        /// If no object has been detected, it moves forward in bigger steps until an object has been detected. It then moves a step back and moves forward in smaller steps until the object is detected again.
        /// It then updates the x, y, z, pitch, roll, and yaw values of the measure position with the current position of the robot.
        /// It adds a milling offset to the measure position and moves the robot back to the original position.
        /// Finally, it returns the updated measure positions.
        /// </remarks>
        public IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints)
        {
            List<CobotPosition> newMeasurePositions = GeneratePointsBetween(measurePoints);
            foreach (CobotPosition measurePosition in newMeasurePositions)
            {
                _cobotController.StepSize = _roughStepSize;
                CobotPosition returnPosition = measurePosition.Copy();
                _cobotController.MoveToDirect(measurePosition, _cobotController.MovementSpeed);

                if (measurePosition.PointType != PointTypeDefinition.Dummy)
                {
                    _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                    if (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                    {
                        // Move forward in bigger steps until an object has been detected.
                        _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                        while (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                        {
                            _cobotController.MoveStepToObject(_cobotController.CurrentPosition, MovementDirection.Forward);
                        }

                        // Move a step back.
                        _cobotController.MoveStepToObject(_cobotController.CurrentPosition, MovementDirection.Backward, 3);

                        // Move forward in small steps until the object is detected again.
                        _cobotController.StepSize = _preciseStepSize;
                        _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                        while (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                        {
                            _cobotController.MoveStepToObject(_cobotController.CurrentPosition, MovementDirection.Forward);
                        }

                        CobotPosition objectPosition = _cobotController.CurrentPosition;
                        measurePosition.X = objectPosition.X;
                        measurePosition.Y = objectPosition.Y;
                        measurePosition.Z = objectPosition.Z;
                        measurePosition.Pitch = objectPosition.Pitch;
                        measurePosition.Roll = objectPosition.Roll;
                        measurePosition.Yaw = objectPosition.Yaw;

                        _cobotController.AddMillingOffsetPosition(measurePosition);
                        _cobotController.MoveToDirect(returnPosition, _cobotController.MovementSpeed);
                    }
                }
            }
            return newMeasurePositions;
        }

        /// <summary>
        /// Generates additional points between the given measure points.
        /// </summary>
        /// <param name="measurePoints">A collection of CobotPositions representing the points to generate additional points between.</param>
        /// <returns>A collection of CobotPositions representing the generated points.</returns>
        /// <remarks>
        /// This method first adds the first measure point to the list of generated points.
        /// It then iterates over the remaining measure points. For each position, it checks if it should generate additional points between the current position and the previous one.
        /// If the position type is Line and the PointsToGenerateBetweenLast property is greater than 0, it calculates the distributions for x, y, z, and jaw coordinates and generates the additional points.
        /// If the position type is Corner, it calculates the corner position and adds it to the list of generated points.
        /// Finally, it adds the current position to the list of generated points.
        /// </remarks>
        private List<CobotPosition> GeneratePointsBetween(IEnumerable<CobotPosition> measurePoints)
        {
            List<CobotPosition> generatedPoints = new() { measurePoints.First() };

            for (int i = 1; i < measurePoints.Count(); i++)
            {
                CobotPosition previousPos = measurePoints.ElementAt(i - 1);
                CobotPosition currPos = measurePoints.ElementAt(i);

                if (currPos.PointType == PointTypeDefinition.Line)
                {
                    if (currPos.PointsToGenerateBetweenLast > 0)
                    {
                        float distributionX = (currPos.X - previousPos.X) / (currPos.PointsToGenerateBetweenLast + 1);
                        float distributionY = (currPos.Y - previousPos.Y) / (currPos.PointsToGenerateBetweenLast + 1);
                        float distributionZ = (currPos.Z - previousPos.Z) / (currPos.PointsToGenerateBetweenLast + 1);
                        float distributionJaw = (currPos.Yaw - previousPos.Yaw) / (currPos.PointsToGenerateBetweenLast + 1);

                        for (int j = 0; j < currPos.PointsToGenerateBetweenLast; j++)
                        {
                            generatedPoints.Add(new CobotPosition()
                            {
                                X = generatedPoints.Last().X + distributionX,
                                Y = generatedPoints.Last().Y + distributionY,
                                Z = generatedPoints.Last().Z + distributionZ,
                                Yaw = generatedPoints.Last().Yaw + distributionJaw,
                                Pitch = currPos.Pitch,
                                Roll = currPos.Roll,
                                PointsToGenerateBetweenLast = 0
                            });
                        }
                    }
                }
                else if (currPos.PointType == PointTypeDefinition.Corner)
                {
                    generatedPoints.Add(_positionCalculatorService.GetCornerPosition(previousPos, currPos));
                }

                generatedPoints.Add(currPos);
            }

            return generatedPoints;
        }

        /// <summary>
        /// Updates the positions of the Cobot robot for milling operations.
        /// </summary>
        /// <param name="cobotPositions">A collection of CobotPositions representing the positions to update.</param>
        /// <returns>A collection of CobotPositions representing the updated positions.</returns>
        /// <remarks>
        /// This method first converts the input collection of CobotPositions to a list.
        /// It then iterates over the list. For each position, if the point type is Dummy, it calculates the corner position between the previous and next positions and replaces the current position with this corner position.
        /// Finally, it returns the updated list of positions.
        /// </remarks>
        public List<CobotPosition> UpdatePointsMilling(IEnumerable<CobotPosition> cobotPositions)
        {
            List<CobotPosition> cobotPositionsList = cobotPositions.ToList();

            for (int i = 0; i < cobotPositionsList.Count; i++)
            {
                if (cobotPositionsList[i].PointType == PointTypeDefinition.Dummy)
                {
                    //TODO: Check if the list is long enough
                    CobotPosition[] previousPositions = new CobotPosition[] { cobotPositionsList[i - 1], cobotPositionsList[i - 2] };
                    CobotPosition[] nextPositions = new CobotPosition[] { cobotPositionsList[i + 1], cobotPositionsList[i + 2] };
                    cobotPositionsList[i] = _positionCalculatorService.GetCornerPosition(previousPositions, nextPositions);
                }
            }

            return cobotPositionsList;
        }
    }
}
