using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ValkWelding.Welding.Touch_PoC.Configuration;
using ValkWelding.Welding.Touch_PoC.DistanceDetectors;
using ValkWelding.Welding.Touch_PoC.Services;
using ValkWelding.Welding.Touch_PoC.ViewModels;

namespace ValkWelding.Welding.Touch_PoC
{
    public partial class App : Application
    {
        private IConfiguration _configuration;
        private static ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Add Configuration
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = builder.Build();
            services.Configure<LocalConfig>(_configuration.GetSection("Config"));

            // Add Singleton Services
            services.AddSingleton<MainWindow>();
            services.AddSingleton<IDistanceDetector, TouchDetector>();
            services.AddSingleton<ICobotConnectionService, CobotConnectionService>();
            services.AddSingleton<ICobotControllerService, CobotControllerService>();
            services.AddSingleton<IPathPlanningService, PathPlanningService>();
            services.AddSingleton<IDiskManagementService, DiskManagementService>();

            // Add ViewModels
            services.AddScoped<SettingsViewModel>();
            services.AddScoped<PointListViewModel>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Call startup functions from services that require it
            _serviceProvider.GetService<IPathPlanningService>().Start();

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        public static T GetService<T>() where T : class
        {
            var service = _serviceProvider.GetService(typeof(T)) as T;
            return service!;
        }
    }
}
