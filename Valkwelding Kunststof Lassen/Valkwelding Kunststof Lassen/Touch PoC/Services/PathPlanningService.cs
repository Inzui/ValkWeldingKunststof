using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;
using ValkWelding.Welding.Touch_PoC.ViewModels;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class PathPlanningService : IPathPlanningService
    {
        private ICobotConnectionService _cobotConnectionService;
        private ICobotControllerService _cobotController;
        private IDistanceDetector _distanceDetector;
        private SettingsViewModel _settingsViewModel;

        private System.Timers.Timer _getPositionTimer;
        private bool _busy;

        private float _roughStepSize;
        private float _preciseStepSize;

        public PathPlanningService(IOptions<LocalConfig> configuration, ICobotConnectionService connectionService, ICobotControllerService cobotController, IDistanceDetector distanceDetector, SettingsViewModel settingsViewModel)
        {
            _cobotConnectionService = connectionService;
            _cobotController = cobotController;
            _distanceDetector = distanceDetector;
            _settingsViewModel = settingsViewModel;

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

        private void GetCurrentCobotPositionEvent(object source, ElapsedEventArgs e)
        {
            if (!_busy)
            {
                _busy = true;
                if (_cobotConnectionService.CobotConnected)
                {
                    try
                    {
                        _settingsViewModel.CurrentCobotPosition = _cobotController.GetCobotPosition();
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

        public void ReturnToStartPos(IEnumerable<CobotPosition> cobotPositions)
        {
            _cobotController.StepSize = _roughStepSize;
            IEnumerable<CobotPosition> reversedPositions = cobotPositions.Reverse();
            foreach (CobotPosition cobotPosition in reversedPositions)
            {
                _cobotController.MoveToDirect(cobotPosition);
            }
            _cobotController.StepSize = _roughStepSize*2;
            _cobotController.MoveToDirect(_cobotController.GetBackwardMovementPosition(reversedPositions.Last()));
        }

        public IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints)
        {
            List<CobotPosition> newMeasurePositions = GeneratePointsBetween(measurePoints);
            foreach (CobotPosition measurePosition in newMeasurePositions)
            {
                _cobotController.StepSize = _roughStepSize;
                CobotPosition returnPosition = measurePosition.Copy();
                _cobotController.MoveToDirect(measurePosition);

                _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                if (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                {
                    // Move forward in bigger steps until an object has been detected.
                    _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                    while (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                    {
                        _cobotController.MoveStepToObject(_cobotController.GetCobotPosition(), MovementDirection.Forward);
                    }

                    // Move a step back.
                    _cobotController.MoveStepToObject(_cobotController.GetCobotPosition(), MovementDirection.Backward, 3);

                    // Move forward in small steps until the object is detected again.
                    _cobotController.StepSize = _preciseStepSize;
                    _distanceDetector.SendCommand(DetectorCommand.StartDetecting);
                    while (_distanceDetector.SendCommand(DetectorCommand.RequestObjectDetected) == DetectorResponse.ObjectNotDetected)
                    {
                        _cobotController.MoveStepToObject(_cobotController.GetCobotPosition(), MovementDirection.Forward);
                    }

                    CobotPosition objectPosition = _cobotController.GetCobotPosition();
                    measurePosition.X = objectPosition.X;
                    measurePosition.Y = objectPosition.Y;
                    measurePosition.Z = objectPosition.Z;
                    measurePosition.Pitch = objectPosition.Pitch;
                    measurePosition.Roll = objectPosition.Roll;
                    measurePosition.Yaw = objectPosition.Yaw;
                    _cobotController.MoveToDirect(returnPosition);
                }
            }
            return newMeasurePositions;
        }

        private List<CobotPosition> GeneratePointsBetween(IEnumerable<CobotPosition> measurePoints)
        {
            List<CobotPosition> generatedPoints = new() { measurePoints.First() };

            for (int i = 1; i < measurePoints.Count(); i++)
            {
                CobotPosition previousPos = measurePoints.ElementAt(i - 1);
                CobotPosition currPos = measurePoints.ElementAt(i);

                if (currPos.PointsToGenerateBetweenLast > 0)
                {
                    float distributionX = (currPos.X - previousPos.X) / currPos.PointsToGenerateBetweenLast;
                    float distributionY = (currPos.Y - previousPos.Y) / currPos.PointsToGenerateBetweenLast;
                    float distributionZ = (currPos.Z - previousPos.Z) / currPos.PointsToGenerateBetweenLast;
                    float distributionJaw = (currPos.Yaw - previousPos.Yaw) / currPos.PointsToGenerateBetweenLast;

                    for (int j = 0; j < currPos.PointsToGenerateBetweenLast - 1; j++)
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
                generatedPoints.Add(currPos);
            }

            return generatedPoints;
        }
    }
}
