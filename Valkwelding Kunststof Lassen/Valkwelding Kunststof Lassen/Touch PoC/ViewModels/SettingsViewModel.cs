using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Services;

namespace ValkWelding.Welding.Touch_PoC.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICobotConnectionService CobotConnectionService { get; private set; }

        private ObservableCollection<string> _availableComPorts;
        private SettingsModel _settingsModel;
        private CobotPosition _currentCobotPosition;
        private string _messageBox;
        private bool _connectButtonEnabled;
        private bool _startButtonEnabled;

        public SettingsViewModel(ICobotConnectionService cobotConnectionService)
        {
            CobotConnectionService = cobotConnectionService;
            _settingsModel = new();

            _currentCobotPosition = new()
            {
                X = 0, Y = 0, Z = 0, Pitch = 0, Roll = 0, Yaw = 0
            };
            _messageBox = "Cobot disconnected";
            _connectButtonEnabled = true;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<string> AvailableComPorts
        {
            get
            {
                return _availableComPorts;
            }
            set
            {
                if (value != _availableComPorts)
                {
                    _availableComPorts = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public SettingsModel SettingsModel
        {
            get
            {
                return _settingsModel;
            }
            set
            {
                if (value != _settingsModel)
                {
                    _settingsModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public CobotPosition CurrentCobotPosition
        {
            get
            {
                return _currentCobotPosition;
            }
            set
            {
                if (value != _currentCobotPosition)
                {
                    _currentCobotPosition = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MessageBoxText
        {
            get
            {
                return _messageBox;
            }
            set
            {
                if (value != _messageBox)
                {
                    _messageBox = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool ConnectButtonEnabled
        {
            get
            {
                return _connectButtonEnabled;
            }
            set
            {
                if (value != _connectButtonEnabled)
                {
                    _connectButtonEnabled = value;
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