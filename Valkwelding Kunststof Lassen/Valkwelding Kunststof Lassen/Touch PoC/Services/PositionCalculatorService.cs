using System;
using System.Collections.Generic;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Types;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Options;
using ValkWelding.Welding.Touch_PoC.Configuration;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class PositionCalculatorService
    {
        private Dictionary<char, double> _circleCoordinates;
        private readonly float _yawOffset;

        public PositionCalculatorService(IOptions<LocalConfig> configuration)
        {
            _yawOffset = configuration.Value.CobotSettings.YawOffsetDegrees;
        }

        public CobotPosition GetCornerPosition(CobotPosition positionOne, CobotPosition positionTwo)
        {
            float calibratedYawPositionOne = 450 - positionOne.Yaw + _yawOffset;
            float calibratedYawPositionTwo = 450 - positionTwo.Yaw + _yawOffset;

            if ((calibratedYawPositionOne % 90) == 0)
            {
                calibratedYawPositionOne += 0.1f;
            }

            if ((calibratedYawPositionTwo % 90) == 0)
            {
                calibratedYawPositionTwo += 0.1f;
            }

            //Convert the Yaw degrees into a slope
            double slopeOne = Math.Tan((double)((-calibratedYawPositionOne) * Math.PI / 180.0));
            double slopeTwo = Math.Tan((double)((-calibratedYawPositionTwo) * Math.PI / 180.0));

            //Calculate b value for perpendicular line
            double perpBOne = positionOne.Y - slopeOne * positionOne.X;
            double perpBTwo = positionTwo.Y - slopeTwo * positionTwo.X;

            //Calcualte Intersion points between two perpendicular lines
            double xIntersection = (perpBTwo - perpBOne) / (slopeOne - slopeTwo);
            double yIntersection = (slopeOne * xIntersection) + perpBOne;

            //Create new position where the two perpendicular lines meet
            CobotPosition cornerPosition = positionOne.Copy();
            cornerPosition.X = (float)xIntersection;
            cornerPosition.Y = (float)yIntersection;

            //Calculate new Yaw by taking average of old two yaw positions
            //Check if this works with the yaw positioning (taking 360 degrees into account)
            cornerPosition.Yaw = ((positionOne.Yaw + positionTwo.Yaw) / 2) % 360;
            cornerPosition.PointType = PointTypeDefinition.Dummy;

            return cornerPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="positionsOne">Array of two points</param>
        /// <param name="positionsTwo">Array of two points</param>
        /// <returns>Corner Position</returns>
        public CobotPosition GetCornerPosition(CobotPosition[] positionsOne, CobotPosition[] positionsTwo)
        {
            double[] dV1 = new double[] { positionsOne[0].X, positionsOne[0].Y };
            double[] dV2 = new double[] { positionsTwo[0].X, positionsTwo[0].Y };

            double[] dD1 = new double[] { positionsOne[1].X - positionsOne[0].X, positionsOne[1].Y - positionsOne[0].Y };
            double[] dD2 = new double[] { positionsTwo[1].X - positionsTwo[0].X, positionsTwo[1].Y - positionsTwo[0].Y };

            var v1 = Vector<double>.Build.DenseOfArray(dV1);
            var d1 = Vector<double>.Build.DenseOfArray(dD1);

            var v2 = Vector<double>.Build.DenseOfArray(dV2);
            var d2 = Vector<double>.Build.DenseOfArray(dD2);

            // Define the coefficients of the system of equations
            var A = Matrix<double>.Build.DenseOfArray(new double[,] { { d1[0], -d2[0] }, { d1[1], -d2[1] } });
            var B = v2 - v1;

            // Solve the system of equations
            var solution = A.Solve(B);

            // Extract the values of t and s
            double t = solution[0];
            double s = solution[1];

            // Calculate the point of intersection
            var intersectionPoint = v1 + t * d1;

            CobotPosition cornerPosition = positionsOne[0].Copy();
            cornerPosition.X = (float)intersectionPoint[0];
            cornerPosition.Y = (float)intersectionPoint[1];
            cornerPosition.Yaw = ((positionsOne[0].Yaw + positionsTwo[0].Yaw) / 2) % 360;

            return cornerPosition;
        }

        private void FitCircle(CobotPosition posOne, CobotPosition posTwo, CobotPosition posThree)
        {
            double x12 = posOne.X - posTwo.X;
            double x13 = posOne.X - posThree.X;

            double y12 = posOne.Y - posTwo.Y;
            double y13 = posOne.Y - posThree.Y;

            double y31 = posThree.Y - posOne.Y;
            double y21 = posTwo.Y - posOne.Y;

            double x31 = posThree.X - posOne.X;
            double x21 = posTwo.X - posOne.X;

            double sx13 = (double)(Math.Pow(posOne.X, 2) - Math.Pow(posThree.X, 2));

            double sy13 = (double)(Math.Pow(posOne.Y, 2) - Math.Pow(posThree.Y, 2));

            double sx21 = (double)(Math.Pow(posTwo.X, 2) - Math.Pow(posOne.X, 2));

            double sy21 = (double)(Math.Pow(posTwo.Y, 2) - Math.Pow(posOne.Y, 2));

            double f = ((sx13) * (x12)
                    + (sy13) * (x12)
                    + (sx21) * (x13)
                    + (sy21) * (x13))
                    / (2 * ((y31) * (x12) - (y21) * (x13)));

            double g = ((sx13) * (y12)
                    + (sy13) * (y12)
                    + (sx21) * (y13)
                    + (sy21) * (y13))
                    / (2 * ((x31) * (y12) - (x21) * (y13)));

            double c = -(double)Math.Pow(posOne.X, 2) - (double)Math.Pow(posOne.Y, 2) - 2 * g * posOne.X - 2 * f * posOne.Y;
            double h = -g;
            double k = -f;
            double sqr_of_r = h * h + k * k - c;

            // r is the radius
            double r = Math.Round(Math.Sqrt(sqr_of_r), 5);

            _circleCoordinates = new Dictionary<char, double>
            {
                { 'h', h },
                { 'k', k },
                { 'r', r }
            };
        }

        private void FitCircle(CobotPosition posOne, CobotPosition posTwo, double radius)
        {
            throw new NotImplementedException();
            //TODO: Add fitcircle function for two positions and radius
        }

        /// <summary>
        /// Calculates the equation for a circle, next generates the given amount
        /// of points on that circle.
        /// </summary>
        /// <param name="posOne">First position on the circle, needs to be most right or left</param>
        /// <param name="posTwo">Second position on the circle, needs to lie between posOne and posThree</param>
        /// <param name="posThree">Third position on the circle, needs to be most right or left</param>
        /// <param name="noOfPoints">Amount of points that need to be returned</param>
        /// <returns>A list with points on the circle with the lenght of noOfPoints</returns>
        public List<CobotPosition> GetPointsOnCircle(CobotPosition posOne, CobotPosition posTwo, CobotPosition posThree, int noOfPoints)
        {
            FitCircle(posOne, posTwo, posThree);
            //Center of circle is (h, k) with radius r

            //Check if both points are on the top half of the circle
            if (posOne.Y >= _circleCoordinates['k'] && posThree.Y >= _circleCoordinates['k'])
            {
                return GetPointsOnSameHorizontalHalfCircle(posOne, posThree, noOfPoints, true);
            }
            //Check if both points are on the bottom half of the circle
            else if (posOne.Y < _circleCoordinates['k'] && posThree.Y < _circleCoordinates['k'])
            {
                return GetPointsOnSameHorizontalHalfCircle(posOne, posThree, noOfPoints, false);
            }
            //Check if both points are on the right half of the circle
            else if (posOne.X >= _circleCoordinates['h'] && posThree.X >= _circleCoordinates['h'])
            {
                return GetPointsOnSameVerticalHalfCircle(posOne, posThree, noOfPoints, true);
            }
            //Check if both points are on the left half of the circle
            else if (posOne.X < _circleCoordinates['h'] && posThree.X < _circleCoordinates['h'])
            {
                return GetPointsOnSameVerticalHalfCircle(posOne, posThree, noOfPoints, false);
            }

            throw new ArgumentException("Two the same points were given as argument.");
            // TODO: Discuss if posOne and posThree also need to be added to the list
        }

        private List<CobotPosition> GetPointsOnSameVerticalHalfCircle(CobotPosition posOne, CobotPosition posThree, int noOfPoints, bool right)
        {
            List<CobotPosition> points = new List<CobotPosition>();
            float yDelta = Math.Abs(posThree.Y - posOne.Y) / noOfPoints;
            //If posOne is lower than posThree, add the points bottom to top.
            if (posOne.Y < posThree.Y)
            {
                for (int i = 1; i <= noOfPoints; i++)
                {
                    CobotPosition newPosition = posOne.Copy();
                    newPosition.Y = newPosition.Y + (i * yDelta);
                    newPosition.X = GetCircleXCoordinate(newPosition.Y, right);
                    SetYawPositionCircle(ref newPosition);

                    points.Add(newPosition);
                }
            }
            //If posOne is higher than posThree, add the points top to bottom.
            else
            {
                for (int i = noOfPoints; i < 0; i--)
                {
                    CobotPosition newPosition = posOne.Copy();
                    newPosition.Y = newPosition.Y - (i * yDelta);
                    newPosition.X = GetCircleXCoordinate(newPosition.Y, right);
                    SetYawPositionCircle(ref newPosition);

                    points.Add(newPosition);
                }
            }

            return points;
        }

        private List<CobotPosition> GetPointsOnSameHorizontalHalfCircle(CobotPosition posOne, CobotPosition posThree, int noOfPoints, bool upper)
        {
            List<CobotPosition> points = new List<CobotPosition>();
            float xDelta = Math.Abs(posThree.X - posOne.X) / noOfPoints;
            //If posOne is left of posThree, add the points left to right.
            if (posOne.X < posThree.X)
            {
                for (int i = 1; i <= noOfPoints; i++)
                {
                    CobotPosition newPosition = posOne.Copy();
                    newPosition.X = newPosition.X + (i * xDelta);
                    newPosition.Y = GetCircleYCoordinate(newPosition.X, upper);
                    SetYawPositionCircle(ref newPosition);

                    points.Add(newPosition);
                }
            }
            //If posOne is right of posThree, add the points right to left.
            else
            {
                for (int i = noOfPoints; i < 0; i--)
                {
                    CobotPosition newPosition = posOne.Copy();
                    newPosition.X = newPosition.X - (i * xDelta);
                    newPosition.Y = GetCircleYCoordinate(newPosition.X, upper);
                    SetYawPositionCircle(ref newPosition);

                    points.Add(newPosition);
                }
            }

            return points;
        }

        private float GetCircleYCoordinate(double x, bool upper)
        {
            double discriminant = _circleCoordinates['r'] * _circleCoordinates['r'] - Math.Pow(x - _circleCoordinates['h'], 2);

            // Check if x is within the circle's domain
            if (discriminant < 0)
            {
                throw new ArgumentException("No possible y value for x parameter");
            }

            // Upper half of circle
            double y1 = _circleCoordinates['k'] + Math.Sqrt(discriminant);

            // Lower half of circle
            double y2 = _circleCoordinates['k'] - Math.Sqrt(discriminant);

            if (upper)
            {
                return (float)y1;
            }
            else
            {
                return (float)y2;
            }

        }

        private float GetCircleXCoordinate(float y, bool right)
        {
            double discriminant = _circleCoordinates['r'] * _circleCoordinates['r'] - Math.Pow(y - _circleCoordinates['k'], 2);

            // Check if y is within the circle's domain
            if (discriminant < 0)
            {
                throw new ArgumentException("No possible x value for x parameter");
            }

            // Right half of circle
            double x1 = _circleCoordinates['h'] + Math.Sqrt(discriminant);

            // Left half of circle
            double x2 = _circleCoordinates['h'] - Math.Sqrt(discriminant);

            if (right)
            {
                return (float)x1;
            }
            else
            {
                return (float)x2;
            }
        }

        private void SetYawPositionCircle(ref CobotPosition pos)
        {
            double deltaX = _circleCoordinates['h'] - pos.X;
            double deltaY = _circleCoordinates['k'] - pos.Y;

            double yawRad = Math.Atan2(deltaY, deltaX);
            pos.Yaw = (float)(yawRad * (180 / Math.PI));
        }
    }
}
