using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.Services;

namespace ValkWelding.Welding.Touch_PoC
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IDistanceDetector, TouchDetector>();

            services.AddScoped<IDetectionService, DetectionService>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _serviceProvider.GetService<IDistanceDetector>().Start();

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
    }
}
