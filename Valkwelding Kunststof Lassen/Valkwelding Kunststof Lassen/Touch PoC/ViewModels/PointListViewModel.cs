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

        private bool _addButtonEnabled;
        private bool _startButtonEnabled;

        public PointListViewModel() 
        {
            _cobotPositions = new();
            AddButtonEnabled = true;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #warning FIX REMOVE
        public void RemovePositionFromList(CobotPosition pos)
        {
            _cobotPositions.Remove(_cobotPositions.First(x => x.Id == pos.Id));
            SelectedPosition = null;
            
            for (int i = pos.Id;  i < _cobotPositions.Count; i++)
            {
                CobotPositions.ElementAt(i).Id = i;
            }
            OnPropertyChanged();
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
        
        public bool AddButtonEnabled
        {
            get
            {
                return _addButtonEnabled;
            }
            set
            {
                if (value != _addButtonEnabled)
                {
                    _addButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool StartButtonEnabled
        {
            get
            {
                return _startButtonEnabled;
            }
            set
            {
                if (value != _startButtonEnabled)
                {
                    _startButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
