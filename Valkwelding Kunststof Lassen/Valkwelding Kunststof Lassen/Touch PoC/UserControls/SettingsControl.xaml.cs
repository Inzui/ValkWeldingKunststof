using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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

        public SettingsControl()
        {
            DataContext = App.GetService<SettingsViewModel>();
            _pointListViewModel = App.GetService<PointListViewModel>();
            _pathPlanningService = App.GetService<IPathPlanningService>();
            _cobotConnectionService = App.GetService<ICobotConnectionService>();
            _cobotControllerService = App.GetService<ICobotControllerService>();

            InitializeComponent();
        }

        private async void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.MessageBoxText = "Connecting...";
                ViewModel.ConnectButtonEnabled = false;
                await ViewModel.CobotConnectionService.CheckConnection(ViewModel.CobotIpAddress);
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
                        _pathPlanningService.ReturnToStartPos(_pointListViewModel.MeasuredPositions);
                    });
                    
                    ViewModel.MessageBoxText = "Milling...";
                    await Task.Run(() =>
                    {
                        _cobotControllerService.StartMillSequence(_pointListViewModel.MeasuredPositions);
                    });
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
    }
}
