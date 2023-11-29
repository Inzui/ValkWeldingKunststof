﻿using Microsoft.Extensions.Options;
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

        public IEnumerable<CobotPosition> Detect(IEnumerable<CobotPosition> measurePoints, int amountOfPoints)
        {
            List<CobotPosition> newMeasurePositions = GeneratePointsBetween(measurePoints, amountOfPoints);
            foreach (CobotPosition measurePosition in newMeasurePositions)
            {
                CobotPosition returnPosition = measurePosition.Copy();
                _cobotController.MoveToDirect(measurePosition);

                if (!_distanceDetector.ObjectDetected)
                {
                    // Move forward in bigger steps until an object has been detected.
                    _cobotController.StepSize = _roughStepSize;
                    while (!_distanceDetector.ObjectDetected)
                    {
                        _cobotController.MoveStepToObject(_cobotController.GetCobotPosition(), MovementDirection.Forward);
                    }
                    // Move backward in bigger steps until the object is no longer detected.
                    while (_distanceDetector.ObjectDetected)
                    {
                        _cobotController.MoveStepToObject(_cobotController.GetCobotPosition(), MovementDirection.Backward);
                    }
                    // Move forward in small steps until the object is detected again.
                    _cobotController.StepSize = _preciseStepSize;
                    while (!_distanceDetector.ObjectDetected)
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

        private List<CobotPosition> GeneratePointsBetween(IEnumerable<CobotPosition> measurePoints, int amountOfPoints)
        {
            List<CobotPosition> generatedPoints = new() { measurePoints.First() };

            for (int i = 1; i < measurePoints.Count(); i++)
            {
                CobotPosition previousPos = measurePoints.ElementAt(i - 1);
                CobotPosition currPos = measurePoints.ElementAt(i);

                if (currPos.GeneratePointsBetweenLast)
                {
                    float distributionX = (currPos.X - previousPos.X) / amountOfPoints;
                    float distributionY = (currPos.Y - previousPos.Y) / amountOfPoints;
                    float distributionZ = (currPos.Z - previousPos.Z) / amountOfPoints;
                    float distributionJaw = (currPos.Yaw - previousPos.Yaw) / amountOfPoints;

                    for (int j = 0; j < amountOfPoints - 1; j++)
                    {
                        generatedPoints.Add(new CobotPosition()
                        {
                            X = generatedPoints.Last().X + distributionX,
                            Y = generatedPoints.Last().Y + distributionY,
                            Z = generatedPoints.Last().Z + distributionZ,
                            Yaw = generatedPoints.Last().Yaw + distributionJaw,
                            Pitch = currPos.Pitch,
                            Roll = currPos.Roll,
                            GeneratePointsBetweenLast = false
                        });
                    }
                }
                generatedPoints.Add(currPos);
            }

            return generatedPoints;
        }
    }
}
