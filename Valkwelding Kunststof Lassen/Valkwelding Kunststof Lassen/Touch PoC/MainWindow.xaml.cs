﻿using System;
using System.Collections.Generic;
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

namespace ValkWelding.Welding.Touch_PoC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDistanceDetector _distanceDetector;
        private IPathPlanningService _detectionService;

        public MainWindow(IDistanceDetector detector, IPathPlanningService detectionService)
        {
            _distanceDetector = detector;
            _detectionService = detectionService;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<CobotPosition> positions = new() 
            { 
                new() { X = 300, Y = -300, Z = 250, Roll = 0, Pitch = 180, Yaw = 0},
                new() { X = 350, Y = -300, Z = 250, Roll = 0, Pitch = 180, Yaw = 45, GeneratePointsBetweenLast = false},
                new() { X = 400, Y = -300, Z = 250, Roll = 0, Pitch = 180, Yaw = 90, GeneratePointsBetweenLast = false},
                new() { X = 450, Y = -300, Z = 250, Roll = 0, Pitch = 180, Yaw = 135, GeneratePointsBetweenLast = false}
            };

            _detectionService.Detect(positions, 1);
        }

        private void Detection_Trigger_Down(object sender, RoutedEventArgs e)
        {
            _distanceDetector.ObjectDetected = true;
        }

        private void Detection_Trigger_Up(object sender, RoutedEventArgs e)
        {
            _distanceDetector.ObjectDetected = false;
        }
    }
}
