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
using ValkWelding.Welding.Touch_PoC.Services;

namespace ValkWelding.Welding.Touch_PoC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDetectionService _detectionService;

        public MainWindow(IDetectionService detectionService)
        {
            _detectionService = detectionService;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _detectionService.Detect();
        }
    }
}
