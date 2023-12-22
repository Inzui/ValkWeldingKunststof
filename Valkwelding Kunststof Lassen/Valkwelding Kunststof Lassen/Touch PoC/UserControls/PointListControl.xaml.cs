using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
using ValkWelding.Welding.Touch_PoC.HelperObjects;
using ValkWelding.Welding.Touch_PoC.Services;
using ValkWelding.Welding.Touch_PoC.ViewModels;

namespace ValkWelding.Welding.Touch_PoC.UserControls
{
    /// <summary>
    /// Interaction logic for PointListControl.xaml
    /// </summary>
    public partial class PointListControl : UserControl
    {
        public PointListViewModel ViewModel { get => (PointListViewModel)DataContext; }

        private readonly SettingsViewModel _settingsViewModel;
        private readonly IPathPlanningService _pathPlanningService;
        private readonly ICobotConnectionService _cobotConnectionService;
        private readonly IDiskManagementService _diskManagementService;
        private readonly IDistanceDetector _distanceDetector;

        public PointListControl()
        {
            DataContext = App.GetService<PointListViewModel>();
            _settingsViewModel = App.GetService<SettingsViewModel>();
            _pathPlanningService = App.GetService<IPathPlanningService>();
            _cobotConnectionService = App.GetService<ICobotConnectionService>();
            _diskManagementService = App.GetService<IDiskManagementService>();
            _distanceDetector = App.GetService<IDistanceDetector>();

            InitializeComponent();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            CobotPosition selectedPos = _settingsViewModel.CurrentCobotPosition.Copy();
            selectedPos.Id = ViewModel.ToMeasurePositions.Count;
            ViewModel.ToMeasurePositions.Add(selectedPos);
        }

        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemovePositionFromList(ViewModel.SelectedPosition);
        } 

        private void Import_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsViewModel.MessageBoxText = "Importing Point List...";
                ViewModel.ToMeasurePositions = new ObservableCollection<CobotPosition>(_diskManagementService.ImportPositions());
            }
            catch (IOException) 
            {
                _settingsViewModel.MessageBoxText = "Import Failed: file already opened by other process";
            }
            catch
            {
                _settingsViewModel.MessageBoxText = "Import Failed";
            }
        }
        
        private void Export_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _settingsViewModel.MessageBoxText = "Exporting Point List...";
                _diskManagementService.ExportPositions(ViewModel.ToMeasurePositions);
                _settingsViewModel.MessageBoxText = "Export Success";
            }
            catch (IOException)
            {
                _settingsViewModel.MessageBoxText = "Export Failed: file already opened by other process";
            }
            catch
            {
                _settingsViewModel.MessageBoxText = "Export Failed";
            }
        }
        
        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ToMeasurePositions.Count >= 2)
            {
                try
                {
                    if (!_cobotConnectionService.CobotConnected)
                    {
                        _settingsViewModel.MessageBoxText = "Cobot not connected";
                        return;
                    }
                    if (!_cobotConnectionService.CobotInRunMode)
                    {
                        _settingsViewModel.MessageBoxText = "Cobot not in Run Mode";
                        return;
                    }
                    if (!_distanceDetector.Connected)
                    {
                        _settingsViewModel.MessageBoxText = "Sensor not connected";
                        return;
                    }

                    _settingsViewModel.StartButtonEnabled = false;
                    ViewModel.ButtonsEnabled = false;
                    ViewModel.GridReadOnly = true;

                    _settingsViewModel.MessageBoxText = "Returning to starting position...";
                    ObservableCollection<CobotPosition> measuredPositions = ViewModel.ToMeasurePositions;
                    await Task.Run(() =>
                    {
                        _pathPlanningService.ReturnToStartPos(measuredPositions, true);
                    });

                    _settingsViewModel.MessageBoxText = "Running Measurements...";
                    await Task.Run(() =>
                    {
                        measuredPositions = new(_pathPlanningService.Detect(measuredPositions));
                    });
                    ViewModel.MeasuredPositions = measuredPositions;
                    _settingsViewModel.StartButtonEnabled = true;
                    _settingsViewModel.MessageBoxText = "Measurements Done";
                }
                catch (Exception ex)
                {
                    _settingsViewModel.MessageBoxText = ex.Message;
                }
                finally
                {
                    ViewModel.ButtonsEnabled = true;
                    ViewModel.GridReadOnly = false;
                }
            }
        }
    }
}
