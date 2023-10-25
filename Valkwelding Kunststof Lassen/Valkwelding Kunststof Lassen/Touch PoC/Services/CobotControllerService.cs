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

        public void moveToSteps(float[] point)
        {
            float[] currentPos = roundedPoint(_cob.readPos());
            float[] desPos = roundedPoint(point);

            this._precisionSize = 0;

            while (!onPosition(currentPos, desPos))
            {
                _cob.sendCobotMove(getMove(currentPos, desPos), Speed);
                Thread.Sleep(2000);
                currentPos = roundedPoint(_cob.readPos());
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
