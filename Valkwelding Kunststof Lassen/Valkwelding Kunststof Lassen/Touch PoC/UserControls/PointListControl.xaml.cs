﻿using System;
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

        public PointListControl()
        {
            DataContext = App.GetService<PointListViewModel>();
            _settingsViewModel = App.GetService<SettingsViewModel>();
            _pathPlanningService = App.GetService<IPathPlanningService>();
            _cobotConnectionService = App.GetService<ICobotConnectionService>();

            InitializeComponent();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            CobotPosition selectedPos = _settingsViewModel.CurrentCobotPosition.Copy();
            selectedPos.Id = ViewModel.CobotPositions.Count;
            ViewModel.CobotPositions.Add(selectedPos);
        }

        private void Remove_Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RemovePositionFromList(ViewModel.SelectedPosition);
        } 
        
        private async void Start_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CobotPositions.Count >= 2)
            {
                try
                {
                    if (_cobotConnectionService.CobotInRunMode && _cobotConnectionService.CobotConnected)
                    {
                        ViewModel.ButtonsEnabled = false;
                        ViewModel.GridReadOnly = true;

                        ObservableCollection<CobotPosition> cobotPositions = ViewModel.CobotPositions;
                        _settingsViewModel.MessageBoxText = "Running measurements...";
                        await Task.Run(() =>
                        {
                            _pathPlanningService.Detect(cobotPositions, 3);
                        });
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
