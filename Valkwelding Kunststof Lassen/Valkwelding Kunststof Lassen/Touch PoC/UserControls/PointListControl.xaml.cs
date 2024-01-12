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
using ValkWelding.Welding.PolyTouchApplication.DistanceDetectors;
using ValkWelding.Welding.PolyTouchApplication.HelperObjects;
using ValkWelding.Welding.PolyTouchApplication.Services;
using ValkWelding.Welding.PolyTouchApplication.ViewModels;

namespace ValkWelding.Welding.PolyTouchApplication.UserControls
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

        /// <summary>
        /// Adds a copy of the current Cobot position to the list of positions to measure.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Routed event data.</param>
        /// <remarks>
        /// This method copies the current Cobot position and assigns it an ID equal to the count of positions to measure.
        /// It then adds the copied position to the list of positions to measure.
        /// </remarks>
        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            CobotPosition selectedPos = _settingsViewModel.CurrentCobotPosition.Copy();
            selectedPos.Id = ViewModel.ToMeasurePositions.Count;
            ViewModel.ToMeasurePositions.Add(selectedPos);
        }

        /// <summary>
        /// Removes the selected position from the list of positions to measure.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Routed event data.</param>
        /// <remarks>
        /// This method removes the selected position from the list of positions to measure.
        /// </remarks>
        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemovePositionFromList(ViewModel.SelectedPosition);
        }

        /// <summary>
        /// Imports a list of positions from a file.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Routed event data.</param>
        /// <remarks>
        /// This method first sets the MessageBoxText to "Importing Point List...".
        /// It then attempts to import a list of positions from a file and assigns it to ToMeasurePositions.
        /// If an IOException occurs, it sets the MessageBoxText to "Import Failed: file already opened by other process".
        /// If any other exception occurs, it sets the MessageBoxText to "Import Failed".
        /// </remarks>
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

        /// <summary>
        /// Exports a list of positions to a file.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Routed event data.</param>
        /// <remarks>
        /// This method first sets the MessageBoxText to "Exporting Point List...".
        /// It then exports the list of positions to a file.
        /// If an IOException occurs, it sets the MessageBoxText to "Export Failed: file already opened by other process".
        /// If any other exception occurs, it sets the MessageBoxText to "Export Failed".
        /// </remarks>
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

        /// <summary>
        /// Starts the measurement process.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Routed event data.</param>
        /// <remarks>
        /// This method first checks if there are at least two positions to measure.
        /// If there are, it first checks if the Cobot robot is connected, in run mode, and the sensor is connected. If any of these checks fail, it sets the MessageBoxText accordingly and returns.
        /// It then disables the start button, buttons, and sets the grid to read-only mode.
        /// It sets the MessageBoxText to "Returning to starting position..." and runs a task to return the robot to the starting position.
        /// It sets the MessageBoxText to "Running Measurements..." and runs a task to detect objects at the measure points and adjust their positions.
        /// It sets the measured positions to the updated positions and enables the start button.
        /// It sets the MessageBoxText to "Measurements Done".
        /// If an exception occurs during this process, it sets the MessageBoxText to the exception message.
        /// Finally, it enables the buttons and disables the grid read-only mode.
        /// </remarks>
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
