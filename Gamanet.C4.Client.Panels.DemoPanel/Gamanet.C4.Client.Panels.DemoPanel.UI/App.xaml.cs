
using Gamanet.C4.Client.Panels.DemoPanel.DataSources;
using Gamanet.C4.Client.Panels.DemoPanel.Repositories;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

#if USE_APP_HOSTING
using Microsoft.Extensions.Hosting;
#endif

using System.Windows;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

#if USE_APP_HOSTING
        public static IHost AppHost { get; private set; }
#endif

        static App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => LogException("Current App Domain", s, e.ExceptionObject as Exception); ;
            TaskScheduler.UnobservedTaskException += (s, e) => LogException("Task Scheduler", s, e.Exception); ;

#if USE_APP_HOSTING

            // An Application Host has many advantages (but not necessary for DI only):
            // - Logging
            // - Configuration (Provider) Setup
            // - Environment setup (Development, Staging, Production)
            // - Background services
            // - Using Environment Variables
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                    ServiceProvider = services.BuildServiceProvider();
                })
                .Build();
#else
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
#endif
        }

        public App()
        {
            this.DispatcherUnhandledException += (s, e) => LogException("Dispatcher", s, e.Exception);
        }

        private static void LogException(string source, object sender, Exception? ex)
            => Trace.TraceError($"Source: {source}; Sender: {sender}; Exception: {ex?.Message}", ex);

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register your services here

            // For now we will use only a single instances of default implementations.
            // Change implementation type (not interface type!) here if necessary.
            services.AddSingleton<IPersonDataSource, ExcelPersonDataSource>();
            services.AddSingleton<IPersonRepository, PersonRepository>();
            services.AddSingleton<IDemoPanelContext, _DemoPanelContext>();

            // Using in scope: useful if service implements IDisposable
            services.AddScoped<IFileDialogService, FileDialogService>();

            // Transient: New instance on every request by generic extension method:
            /// <see cref="DependencyInjection.IServiceProvider.GetRequiredService(Type)"/>
            /// or by non-generic non-extension method:
            /// <seealso cref="IServiceProvider.GetService(Type)"/>
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainPanelViewModel>();
            // Only for testing purposes:
            // !!! NOTE !!!: Single instance of MainWindowViewModel would be OK,
            // but not for MainPanelViewModel since the sense of a UserControl
            // re-using multiple times und thus multi-instantiating of it's view model
            // Data context of UserControl is being attached to
            //services.AddSingleton<MainPanelViewModel>();
        }

#if USE_APP_HOSTING
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost.StartAsync();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();

            base.OnExit(e);
        }
#endif

    }
}

