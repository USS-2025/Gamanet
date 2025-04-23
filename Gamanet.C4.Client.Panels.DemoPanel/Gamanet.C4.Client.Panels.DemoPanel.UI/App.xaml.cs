
// If set, (Web) Application hosting with all its nice features will be used like in an ASP.NET app by default
#define USE_APP_HOSTING

using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Model;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources;



#if USE_APP_HOSTING

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#endif

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if USE_APP_HOSTING

        public static IHost AppHost { get; private set; }

        static App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Register your services here

                    // For now we will use only a single instance of IDemoPanelContext's default implementation
                    services.AddSingleton<IDemoPanelContext, _DemoPanelContext>();
                    services.AddSingleton<IPersonDataSource, CsvPersonDataSource>();
                    services.AddSingleton<,>();
                    services.AddSingleton<,>();
                    services.AddSingleton<,>();
                    services.AddSingleton<,>();
                    services.AddSingleton<,>();


                })
                .Build();
        }

#endif

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            // In case of not using App Host and DI (Dependency Injection):
            // Initialize application context manually
            var dpContext = new _DemoPanelContext();


            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();

            base.OnExit(e);
        }
    }

}
