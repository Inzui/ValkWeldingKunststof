using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Animation;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using System.Transactions;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class CobotControllerService : ICobotControllerService
    {
        private int _precisionSize;


        private ICobotConnectionService _cob;
        public int Speed { get; set; }
        public float StepSize { get; set; }

        public CobotControllerService(IOptions<LocalConfig> configuration, ICobotConnectionService cobotConnect)
        {
            this.StepSize = configuration.Value.CobotSettings.MovementStepSize;
            this._precisionSize = 0;
            this._cob = cobotConnect;
            Speed = configuration.Value.CobotSettings.MovementSpeed;
        }

        public void MoveToSteps(float[] point)
        {
            float[] currentPos = RoundedPoint(_cob.readPos());
            float[] desPos = RoundedPoint(point);

            this._precisionSize = 0;

            while (!OnPosition(currentPos, desPos))
            {
                _cob.sendCobotMove(GetMove(currentPos, desPos), Speed);
                Thread.Sleep(2000);
                currentPos = RoundedPoint(_cob.readPos());
            }
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

        private float[] GetMove(float[] currentPos, float[] desPos)
        {
            float[] newMove = { 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < 2; i++)
            {
                if (Math.Abs(currentPos[i] - desPos[i]) < StepSize)
                {
                    newMove[i] = (float)Math.Round(currentPos[i] - desPos[i], _precisionSize);
                }
                else if (currentPos[i] < desPos[i])
                {
                    newMove[i] = -StepSize;
                }
                else if (currentPos[i] > desPos[i])
                {
                    newMove[i] = StepSize;
                }
            }

            float temp = newMove[0];
            newMove[0] = newMove[1];
            newMove[1] = temp;

            return newMove;
        }

        private float[] RoundedPoint(float[] point)
        {
            float[] result = new float[point.Length];

            for (int i = 0; i < point.Length; i++)
            {
                result[i] = (float)Math.Round(point[i], _precisionSize);
            }

            return result;
        }

        private bool OnPosition(float[] currentPos, float[] desPos)
        {
            for (int i = 0; i < currentPos.Length; i++)
            {
                if (currentPos[i] != desPos[i])
                {
                    return false;
                }
            }

            return true;
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

        public void DetectObject(CobotPosition startingPosition)
        {
            MoveToDirect(startingPosition);

            CobotPosition currentPosition = startingPosition;
            for (int i = 0; i < 5; i++)
            {
                currentPosition = GetDetectionPosition(currentPosition);
                MoveToDirect(currentPosition);
            }
        }

        private CobotPosition GetDetectionPosition(CobotPosition startingPosition)
        {
            //Place head in right direction
            float alpha = (float)((-startingPosition.Yaw + 90) * Math.PI / 180.0);

            float deltaX = (float)Math.Cos(alpha) * StepSize;
            float deltaY = (float)Math.Sin(alpha) * -StepSize;

            Debug.WriteLine($"alpha: {alpha} -> {deltaX}, {deltaY}");


            CobotPosition newPosition = startingPosition.Copy();
            newPosition.X += deltaX; 
            newPosition.Y += deltaY;

            return newPosition;
        }
    }
}
