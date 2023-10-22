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
using ValkWelding.Welding.Touch_PoC.Services;

namespace ValkWelding.Welding.Touch_PoC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ICobotControllerService _cob;

        public MainWindow(ICobotControllerService cobotControllerService)
        {
            _cob = cobotControllerService;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            float[] point1 = { 600, -400, 250, -179, 0, -90};
            float[] point2 = { 600, 0, 250, -179, 0, -90 };
            float[] point3 = { 600, 0, 500, -179, 0, -90 };
            float[] point4 = { 600, -400, 500, -179, 0, -90 };

            for(int i = 0; i < 3; i++)
            {
                _cob.moveToDirect(point1);
                _cob.moveToDirect(point2);
                _cob.moveToDirect(point3);
                _cob.moveToDirect(point4);
            }

            _cob.moveToSteps(point1);
            //_cob.moveToSteps(point2);

        }
    }
}
