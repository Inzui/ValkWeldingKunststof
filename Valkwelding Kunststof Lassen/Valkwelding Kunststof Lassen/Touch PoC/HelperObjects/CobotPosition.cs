using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.HelperObjects
{
    public class CobotPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Yaw { get; set; }
        public bool GeneratePointsBetweenLast {  get; set; }

        public override string ToString() 
        {
            return $"X: {X}, Y: {Y}, Z: {Z}, Roll: {Roll}, Pitch: {Pitch}, Yaw: {Yaw}";
        }

        public static bool operator ==(CobotPosition c1, CobotPosition c2)
        {
            return c1.X == c2.X &&
                c1.Y == c2.Y &&
                c1.Z == c2.Z &&
                c1.Pitch == c2.Pitch &&
                c1.Roll == c2.Roll &&
                c1.Yaw == c2.Yaw;
        }

        public static bool operator !=(CobotPosition c1, CobotPosition c2)
        {
            return c1.X != c2.X ||
                c1.Y != c2.Y ||
                c1.Z != c2.Z ||
                c1.Pitch != c2.Pitch ||
                c1.Roll != c2.Roll ||
                c1.Yaw != c2.Yaw;
        }

        public void RoundValues(int digits = 1)
        {
            X = (float)Math.Round(X, digits);
            Y = (float)Math.Round(Y, digits);
            Z = (float)Math.Round(Z, digits);
            Roll = ((float)Math.Round(Roll, digits)) % 360;
            Pitch = ((float)Math.Round(Pitch, digits)) % 360;
            Yaw = ((float)Math.Round(Yaw, digits)) % 360;
        }

        public CobotPosition Copy()
        {
            return (CobotPosition)this.MemberwiseClone();
        }
    }
}
