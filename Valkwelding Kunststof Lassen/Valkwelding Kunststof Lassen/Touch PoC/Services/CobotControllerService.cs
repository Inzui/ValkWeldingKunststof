using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Media.Animation;

namespace ValkWelding.Welding.Touch_PoC.Services
{
    public class CobotControllerService : ICobotControllerService
    {
        private int _precisionSize;


        private ICobotConnectionService _cob;
        public int Speed { get; set; }
        public float StepSize { get; set; }

        public CobotControllerService(ICobotConnectionService cobotConnect)
        {
            this.StepSize = 5;
            this._precisionSize = 2;
            this._cob = cobotConnect;
            Speed = 100;
        }

        public void moveToSteps(float[] point)
        {
            float[] currentPos = roundedPoint(_cob.readPos());
            float[] desPos = roundedPoint(point);

            while (!onPosition(currentPos, desPos))
            {
                _cob.sendCobotMove(getMove(currentPos, desPos), Speed);
                currentPos = roundedPoint(_cob.readPos());
                Thread.Sleep(1000);
                Trace.WriteLine("--------------------");
                printPoint(currentPos);
                printPoint(desPos);
            }
        }

        public void moveToDirect(float[] point)
        {
            float[] currentPos = roundedPoint(_cob.readPos());
            float[] desPos = roundedPoint(point);

            _cob.sendCobotPos(desPos, Speed);

            Trace.WriteLine(_cob.readError());

            while (!onPosition(currentPos, desPos))
            {
                currentPos = roundedPoint(_cob.readPos());
            }
        }

        private float[] getMove(float[] currentPos, float[] desPos)
        {
            float[] newMove = new float[6];

            for (int i = 0; i < newMove.Length; i++)
            {
                if (Math.Abs(currentPos[i] - desPos[i]) < StepSize)
                {
                    newMove[i] = currentPos[i] - desPos[i];
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

        private float[] roundedPoint(float[] point)
        {
            float[] result = new float[point.Length];

            for (int i = 0; i < point.Length; i++)
            {
                result[i] = (float)Math.Round(point[i], _precisionSize);
            }

            return result;
        }

        private bool onPosition(float[] currentPos, float[] desPos)
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

        private void printPoint(float[] point)
        {
            foreach (var p in point)
            {
                Trace.Write($"{p}, ");
            }
            Trace.WriteLine("");
        }
    }
}
