using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.Services;
using ValkWelding.Welding.Touch_PoC.ViewModels;

namespace ValkWelding.Welding.Touch_PoC.UserControls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsViewModel ViewModel { get => (SettingsViewModel)DataContext; }

        private readonly PointListViewModel _pointListViewModel;
        private readonly IPathPlanningService _pathPlanningService;
        private readonly ICobotConnectionService _cobotConnectionService;
        private readonly ICobotControllerService _cobotControllerService;
        private readonly IDistanceDetector _distanceDetector;
        private readonly IDiskManagementService _diskManagementService;

        public SettingsControl()
        {
            DataContext = App.GetService<SettingsViewModel>();
            _pointListViewModel = App.GetService<PointListViewModel>();
            _pathPlanningService = App.GetService<IPathPlanningService>();
            _cobotConnectionService = App.GetService<ICobotConnectionService>();
            _cobotControllerService = App.GetService<ICobotControllerService>();
            _distanceDetector = App.GetService<IDistanceDetector>();
            _diskManagementService = App.GetService<IDiskManagementService>();

            InitializeComponent();
            Loaded += (s, e) =>
            {
                Window.GetWindow(this).Closing += (s1, e1) => Stop();
            };
            Start();
        }

        public void Start()
        {
            ViewModel.AvailableComPorts = new(SerialPort.GetPortNames());
            ViewModel.SettingsModel = _diskManagementService.LoadSettings();
            if (string.IsNullOrEmpty(ViewModel.SettingsModel.SelectedComPort))
            {
                ViewModel.SettingsModel.SelectedComPort = ViewModel.AvailableComPorts.FirstOrDefault();
            }
        }

        protected  void Stop()
        {
            _diskManagementService.WriteSettings(ViewModel.SettingsModel);
        }

        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_cobotConnectionService.CobotInRunMode && _cobotConnectionService.CobotConnected)
                {
                    ViewModel.StartButtonEnabled = false;
                    _pointListViewModel.ButtonsEnabled = false;

                    ViewModel.MessageBoxText = "Returning to starting position...";
                    await Task.Run(() =>
                    {
                        _pathPlanningService.ReturnToStartPos(_pointListViewModel.MeasuredPositions, false);
                    });

                    ViewModel.MessageBoxText = "Milling...";
                    await Task.Run(() =>
                    {
                        _cobotControllerService.StartMillSequence(_pathPlanningService.UpdatePointsMilling(_pointListViewModel.MeasuredPositions));
                    });

                    ViewModel.MessageBoxText = "Milling Done";
                }
                else
                {
                    ViewModel.MessageBoxText = "Cobot not connected or not in Run Mode";
                }
            }
            catch (Exception ex)
            {
                ViewModel.MessageBoxText = ex.Message;
            }
            finally
            {
                ViewModel.StartButtonEnabled = true;
                _pointListViewModel.ButtonsEnabled = true;
            }
        }

        private async void Connect_Cobot_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.MessageBoxText = "Connecting Cobot...";
                ViewModel.ConnectButtonEnabled = false;
                await ViewModel.CobotConnectionService.CheckConnection(ViewModel.SettingsModel.CobotIpAddress);
                _cobotControllerService.StopMill();
                ViewModel.MessageBoxText = "Connected";
            }
            catch (Exception ex)
            {
                ViewModel.MessageBoxText = ex.Message;
                Debug.WriteLine(ex);
            }
            finally
            {
                ViewModel.ConnectButtonEnabled = true;
            }
        }

        private async void Connect_Sensor_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.MessageBoxText = "Connecting Sensor...";
                ViewModel.ConnectButtonEnabled = false;

                string comPort = ViewModel.SettingsModel.SelectedComPort;
                await Task.Run(() =>
                {
                    _distanceDetector.Connect(comPort);
                });
                ViewModel.MessageBoxText = _distanceDetector.Connected ? "Sensor Connected" : "Sensor Connection Timed Out";
            }
            catch (Exception ex)
            {
                ViewModel.MessageBoxText = ex.Message;
                Debug.WriteLine(ex);
            }
            finally
            {
                ViewModel.ConnectButtonEnabled = true;
            }
        }

        private void Probe_Extrude_Button_Click(object sender, RoutedEventArgs e)
        {
            if (_distanceDetector.Connected)
            {
                ViewModel.MessageBoxText = "Extruding Probe...";
                _distanceDetector.SendCommand(Types.DetectorCommand.StartDetecting);
            }
            else
            {
                ViewModel.MessageBoxText = "Sensor Not Connected";
            }
        }
    }
}
