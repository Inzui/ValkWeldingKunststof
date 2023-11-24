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

        public PointListViewModel() 
        {
            _cobotPositions = new();
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void RemovPositionFromList(CobotPosition pos)
        {
            CobotPositions.Remove(pos);
            SelectedPosition = null;
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
    }
}
