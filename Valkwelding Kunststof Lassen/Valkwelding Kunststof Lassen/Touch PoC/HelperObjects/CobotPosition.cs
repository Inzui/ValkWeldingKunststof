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

        public static bool operator ==(CobotPosition a, CobotPosition b)
        {
            if (a is null)
            {
                if (b is null)
                {
                    return true;
                }
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(CobotPosition a, CobotPosition b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            CobotPosition p = obj as CobotPosition;
            if (p is null)
            {
                return false;
            }

            return (X == p.X) 
                && (Y == p.Y) 
                && (Z == p.Z) 
                && (Pitch == p.Pitch) 
                && (Roll == p.Roll) 
                && (Yaw == p.Yaw) 
                && (GeneratePointsBetweenLast == p.GeneratePointsBetweenLast);
        }

        public bool Equals(CobotPosition p)
        {
            if (p is null)
            {
                return false;
            }
            return (X == p.X) 
                && (Y == p.Y) 
                && (Z == p.Z) 
                && (Pitch == p.Pitch) 
                && (Roll == p.Roll) 
                && (Yaw == p.Yaw) 
                && (GeneratePointsBetweenLast == p.GeneratePointsBetweenLast);
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

        public CobotPosition Copy()
        {
            return (CobotPosition)this.MemberwiseClone();
        }
    }
}
