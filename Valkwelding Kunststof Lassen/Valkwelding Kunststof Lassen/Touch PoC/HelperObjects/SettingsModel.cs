using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ValkWelding.Welding.Touch_PoC.HelperObjects
{
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _selectedComPort;
        private string _cobotIpAddress;

        public SettingsModel() { }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string SelectedComPort
        {
            get
            {
                return _selectedComPort;
            }
            set
            {
                if (value != _selectedComPort)
                {
                    _selectedComPort = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CobotIpAddress
        {
            get
            {
                return _cobotIpAddress;
            }
            set
            {
                if (value != _cobotIpAddress)
                {
                    _cobotIpAddress = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
