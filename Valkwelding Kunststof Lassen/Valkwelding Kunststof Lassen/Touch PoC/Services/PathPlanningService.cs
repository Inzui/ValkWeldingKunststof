using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.ViewModels;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class PathPlanningService : IPathPlanningService
    {
        private ICobotConnectionService _cobotConnectionService;
        private ICobotControllerService _cobotController;
        private SettingsViewModel _settingsViewModel;

        private Timer _getPositionTimer;
        private bool _busy;

        public PathPlanningService(ICobotConnectionService connectionService, ICobotControllerService cobotController, SettingsViewModel settingsViewModel)
        {
            _cobotConnectionService = connectionService;
            _cobotController = cobotController;
            _settingsViewModel = settingsViewModel;

            _getPositionTimer = new();
            _getPositionTimer.Elapsed += new ElapsedEventHandler(GetCurrentCobotPositionEvent);
            _getPositionTimer.Interval = 500;
            _busy = false;
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

        public void Detect(IEnumerable<CobotPosition> measurePoints, int amountOfPoints)
        {
            List<CobotPosition> measurePositions = GeneratePointsBetween(measurePoints, amountOfPoints);
            foreach (CobotPosition measurePosition in measurePositions)
            {
                _cobotController.DetectObject(measurePosition);
            }
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

                    for (int j = 0; j < amountOfPoints -1; j++)
                    {
                        generatedPoints.Add(new CobotPosition()
                        {
                            X = generatedPoints.Last().X + distributionX,
                            Y = generatedPoints.Last().Y + distributionY,
                            Z = generatedPoints.Last().Z + distributionZ,
                            Yaw = generatedPoints.Last().Yaw + distributionJaw,
                            Pitch = currPos.Pitch,
                            Roll = currPos.Roll,
                            GeneratePointsBetweenLast = true
                        });
                    }
                }
                generatedPoints.Add(currPos);
            }

            return generatedPoints;
        }
    }
}
