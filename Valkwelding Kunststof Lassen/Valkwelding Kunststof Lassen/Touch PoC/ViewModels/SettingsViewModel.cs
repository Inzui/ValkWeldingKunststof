using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.Services;

namespace ValkWelding.Welding.Touch_PoC.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICobotConnectionService CobotConnectionService { get; private set; }
        private string _currentVec;
        private string _cobotIpAddress;

        public SettingsViewModel(IOptions<LocalConfig> configuration, ICobotConnectionService cobotConnectionService)
        {
            CobotConnectionService = cobotConnectionService;
            CobotIpAddress = configuration.Value.CobotSettings.IpAddress;
            _currentVec = "VECTOR";
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string CurrentCobotVector
        {
            get
            {
                return _currentVec;
            }
            set
            {
                if (value != _currentVec)
                {
                    _currentVec = value;
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