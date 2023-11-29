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

        public SettingsControl()
        {
            DataContext = App.GetService<SettingsViewModel>();
            InitializeComponent();
        }

        private async void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.MessageBoxText = "Connecting...";
                ViewModel.ConnectButtonEnabled = false;
                await ViewModel.CobotConnectionService.CheckConnection(ViewModel.CobotIpAddress);
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
        }
    }
}
