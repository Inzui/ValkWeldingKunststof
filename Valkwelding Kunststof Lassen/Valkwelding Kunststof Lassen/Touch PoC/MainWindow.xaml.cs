using System;
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
                new() { X = 600, Y = -400, Z = 250, Yaw = 0, Pitch = 90, Roll = 0},
                new() { X = 600, Y = -400, Z = 250, Yaw = -45, Pitch = 90, Roll = 0, GeneratePointsBetweenLast = false}
            };

            _detectionService.Detect(positions, 5);
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
