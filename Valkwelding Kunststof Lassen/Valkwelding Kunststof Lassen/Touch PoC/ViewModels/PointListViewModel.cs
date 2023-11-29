using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ValkWelding.Welding.Touch_PoC.HelperObjects;

namespace ValkWelding.Welding.Touch_PoC.ViewModels
{
    public class PointListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<CobotPosition> _cobotPositions;
        private CobotPosition _selectedPosition;

        private bool _buttonsEnabled;
        private bool _gridReadOnly;

        public PointListViewModel() 
        {
            _cobotPositions = new();
            ButtonsEnabled = true;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RemovePositionFromList(CobotPosition pos)
        {
            _cobotPositions.Remove(pos);
            SelectedPosition = null;
            
            for (int i = pos.Id;  i < _cobotPositions.Count; i++)
            {
                CobotPositions.ElementAt(i).Id = i;
            }
        }

        public ObservableCollection<CobotPosition> CobotPositions
        {
            get
            {
                return _cobotPositions;
            }
            set
            {
                if (value != _cobotPositions)
                {
                    _cobotPositions = value;
                    OnPropertyChanged();
                }
            }
        }

        public CobotPosition SelectedPosition
        {
            get
            {
                return _selectedPosition;
            }
            set
            {
                if (value != _selectedPosition)
                {
                    _selectedPosition = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool ButtonsEnabled
        {
            get
            {
                return _buttonsEnabled;
            }
            set
            {
                if (value != _buttonsEnabled)
                {
                    _buttonsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool GridReadOnly
        {
            get
            {
                return _gridReadOnly;
            }
            set
            {
                if (value != _gridReadOnly)
                {
                    _gridReadOnly = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
