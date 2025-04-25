using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider? _serviceProvider;
        private MainWindowViewModel? _model;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += this.MainWindow_Loaded;
            this.DataContextChanged += this.MainWindow_DataContextChanged;

            _serviceProvider = App.ServiceProvider;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var modelToAttach = _serviceProvider?.GetRequiredService<MainWindowViewModel>() ?? new MainWindowViewModel();
            _model = modelToAttach;

            string infoMsg = $"{nameof(MainWindow_Loaded)}:" +
                $" Attaching {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}...";
            Trace.TraceInformation(infoMsg);

            // Set it only once per MainPanel to have only one instance per MainPanel
            // Getting model instance per service provider will cause constructor DI to work.
            //this.RootContainer.DataContext = _model = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            this.DataContext = modelToAttach;

            infoMsg = $"{nameof(MainWindow_Loaded)}: Attached {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}.";
            Trace.TraceInformation(infoMsg);

            if (this.DataContext is MainPanelViewModel model)
            {
                model.StatusText = infoMsg;
            }
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string infoMsg = $"{nameof(MainWindow_DataContextChanged)}: {e.OldValue ?? "null"} --> {e.NewValue ?? "null"}";
            Trace.TraceInformation(infoMsg);

            if (this.DataContext is not MainWindowViewModel)
            {
                var modelToAttach = _serviceProvider?.GetRequiredService<MainWindowViewModel>() ?? new MainWindowViewModel();
                _model = modelToAttach;

                Trace.TraceInformation($"{nameof(MainWindow_DataContextChanged)}:" +
                    $" Attaching {modelToAttach} to {nameof(DataContext)} of {nameof(MainWindow)}...");

                // Set it only once per MainPanel to have only one instance per MainPanel.
                // Getting model instance per service provider will cause constructor DI to work.
                //this.RootContainer.DataContext = _model = _serviceProvider.GetRequiredService<MainWindowViewModel>();

                // Furthermore, we have to ensure that no later data context change will override this data context
                // since data context will be derived from main window
                this.DataContext = modelToAttach;

                infoMsg = $"{nameof(MainWindow_DataContextChanged)}: Attached {modelToAttach} to {nameof(DataContext)} of {nameof(MainWindow)}.";
                Trace.TraceInformation(infoMsg);

                if (this.DataContext is MainWindowViewModel model)
                {
                    model.StatusText = infoMsg;
                }
            }
        }
    }
}