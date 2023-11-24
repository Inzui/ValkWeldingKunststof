﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.HelperObjects
{
    public class CobotPosition : INotifyPropertyChanged
    {
        private int _id;
        private float _x;
        private float _y;
        private float _z;
        private float _pitch;
        private float _roll;
        private float _yaw;
        private bool _generatePointsBetweenLast;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public float X
        {
            get
            {
                return _x;
            }
            set
            {
                if (value != _x)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Y
        {
            get
            {
                return _y;
            }
            set
            {
                if (value != _y)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Z
        {
            get
            {
                return _z;
            }
            set
            {
                if (value != _z)
                {
                    _z = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Pitch
        {
            get
            {
                return _pitch;
            }
            set
            {
                if (value != _pitch)
                {
                    _pitch = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Roll
        {
            get
            {
                return _roll;
            }
            set
            {
                if (value != _roll)
                {
                    _roll = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Yaw
        {
            get
            {
                return _yaw;
            }
            set
            {
                if (value != _yaw)
                {
                    _yaw = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool GeneratePointsBetweenLast
        {
            get
            {
                return _generatePointsBetweenLast;
            }
            set
            {
                if (value != _generatePointsBetweenLast)
                {
                    _generatePointsBetweenLast = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EqualPosition(CobotPosition otherPos)
        {
            if (otherPos is null)
            {
                return false;
            }
            return (_x == otherPos._x)
                && (_y == otherPos._y)
                && (_z == otherPos._z)
                && (_pitch == otherPos._pitch)
                && (_roll == otherPos._roll)
                && (_yaw == otherPos._yaw);
        }

        public override string ToString() 
        {
            return $"ID: {_id}, X: {_x}, Y: {_y}, Z: {_z}, Roll: {_roll}, Pitch: {_pitch}, Yaw: {_yaw}";
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

            return (_id == p._id)
                && (_x == p._x) 
                && (_y == p._y) 
                && (_z == p._z) 
                && (_pitch == p._pitch) 
                && (_roll == p._roll) 
                && (_yaw == p._yaw) 
                && (_generatePointsBetweenLast == p._generatePointsBetweenLast);
        }

        public bool Equals(CobotPosition p)
        {
            if (p is null)
            {
                return false;
            }
            return (_id == p._id)
                && (_x == p._x) 
                && (_y == p._y) 
                && (_z == p._z) 
                && (_pitch == p._pitch) 
                && (_roll == p._roll) 
                && (_yaw == p._yaw) 
                && (_generatePointsBetweenLast == p._generatePointsBetweenLast);
        }

        public void RoundValues(int digits = 2)
        {
            _x = (float)Math.Round(_x, digits);
            _y = (float)Math.Round(_y, digits);
            _z = (float)Math.Round(_z, digits);
            _roll = ((float)Math.Round(_roll, digits)) % 360;
            _pitch = ((float)Math.Round(_pitch, digits)) % 360;
            _yaw = ((float)Math.Round(_yaw, digits)) % 360;
        }

        public CobotPosition Copy()
        {
            return (CobotPosition)this.MemberwiseClone();
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

    }
}
