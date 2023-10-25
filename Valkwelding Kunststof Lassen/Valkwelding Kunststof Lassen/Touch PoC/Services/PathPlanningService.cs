using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class PathPlanningService : IPathPlanningService
    {
        private ICobotControllerService _cobotController;

        public PathPlanningService(ICobotControllerService cobotController)
        {
            _cobotController = cobotController;
        }

        public void Detect(IEnumerable<CobotPosition> measurePoints, int pointsBetween)
        {
            List<CobotPosition> measurePositions = GeneratePointsBetween(measurePoints, pointsBetween);
            foreach (CobotPosition measurePosition in measurePositions)
            {
                _cobotController.DetectObject(measurePosition);
            }
        }

        private List<CobotPosition> GeneratePointsBetween(IEnumerable<CobotPosition> measurePoints, int pointsBetween)
        {
            List<CobotPosition> generatedPoints = new() { measurePoints.First() };

            for (int i = 1; i < measurePoints.Count(); i++)
            {
                CobotPosition previousPos = measurePoints.ElementAt(i - 1);
                CobotPosition currPos = measurePoints.ElementAt(i);

                if (currPos.GeneratePointsBetweenLast)
                {
                    float distributionX = (currPos.X - previousPos.X) / pointsBetween;
                    float distributionY = (currPos.Y - previousPos.Y) / pointsBetween;
                    float distributionZ = (currPos.Z - previousPos.Z) / pointsBetween;
                    float distributionJaw = (currPos.Yaw - previousPos.Yaw) / pointsBetween;

                    for (int j = 0; j < pointsBetween -1; j++)
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
