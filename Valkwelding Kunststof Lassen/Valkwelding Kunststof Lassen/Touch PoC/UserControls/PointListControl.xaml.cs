using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly ICobotControllerService _cobotControllerService;

        public PointListControl()
        {
            DataContext = App.GetService<PointListViewModel>();
            _settingsViewModel = App.GetService<SettingsViewModel>();
            _pathPlanningService = App.GetService<IPathPlanningService>();
            _cobotConnectionService = App.GetService<ICobotConnectionService>();
            _cobotControllerService = App.GetService<ICobotControllerService>();

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
        
        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ToMeasurePositions.Count >= 2)
            {
                try
                {
                    if (_cobotConnectionService.CobotInRunMode && _cobotConnectionService.CobotConnected)
                    {
                        _settingsViewModel.StartButtonEnabled = false;
                        ViewModel.ButtonsEnabled = false;
                        ViewModel.GridReadOnly = true;

                        _settingsViewModel.MessageBoxText = "Returning to starting position...";
                        ObservableCollection<CobotPosition> measuredPositions = ViewModel.ToMeasurePositions;
                        await Task.Run(() =>
                        {
                            _pathPlanningService.ReturnToStartPos(measuredPositions);
                        });

                        _settingsViewModel.MessageBoxText = "Running measurements...";
                        await Task.Run(() =>
                        {
                            measuredPositions = new(_pathPlanningService.Detect(measuredPositions));
                        });
                        ViewModel.MeasuredPositions = measuredPositions;
                        _settingsViewModel.StartButtonEnabled = true;
                        _settingsViewModel.MessageBoxText = "Measurements done";
                    }
                    else
                    {
                        _settingsViewModel.MessageBoxText = "Cobot not connected or not in Run Mode";
                    }
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
