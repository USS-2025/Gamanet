// If set, (Web) Application hosting with all its nice features will be used like in an ASP.NET app by default

using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        private readonly IServiceProvider _serviceProvider;
        private MainPanelViewModel? _model;

        public MainPanel()
        {
            InitializeComponent();

            this.Loaded += this.MainPanel_Loaded;
            this.DataContextChanged += this.MainPanel_DataContextChanged;
            
            _serviceProvider = App.ServiceProvider;
        }

        private void MainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Set it only once per MainPanel to have only one instance per MainPanel.
            // Getting model instance per service provider will cause constructor DI to work.
            this.RootContainer.DataContext = _model = _serviceProvider.GetRequiredService<MainPanelViewModel>();

            string infoMsg = $"{nameof(MainPanel_Loaded)}: {nameof(DataContext)}=={this.DataContext ?? "null"}";

            Trace.TraceInformation(infoMsg);

            if (this.DataContext is MainPanelViewModel model)
            {
                model.StatusText = infoMsg;
            }
        }

        private void MainPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string infoMsg = $"{nameof(MainPanel_DataContextChanged)}: {e.OldValue ?? "null"} --> {e.NewValue ?? "null"}";

            Trace.TraceInformation(infoMsg);

            if (this.RootContainer.DataContext is MainPanelViewModel model)
            {
                model.StatusText = infoMsg;
            }
        }

        private void CountryFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_model == null)
            {
                return;
            }

            if (sender == this.CountryFilterCombo)
            {
                _model.FilterByCountry((string)CountryFilterCombo.SelectedItem);
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_model == null)
            {
                return;
            }

            switch (sender)
            {
                case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByName):
                    // Sort by name
                    _model.SortByNameToggle();
                    break;

                case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByName):
                    // Sort by name
                    _model.SortByNameToggle();
                    break;

                case Control control:

                    _model.ErrorText =
                        $"Control {control.Name} is not handled in CheckBox_Checked event.";
                    break;

                default:
                    // Handle other controls or log warning
                    _model.ErrorText =
                        $"Sender {sender.GetType().Name} is not handled in CheckBox_Checked event.";
                    break;
            }
        }

        private async void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model == null)
            {
                return;
            }

            await _model.LoadPersons();

            // ComboBox not showing anything will confuse the user
            if (this.CountryFilterCombo.SelectedItem == null
                && this.CountryFilterCombo.Items.Count > 0)
            {
                this.CountryFilterCombo.SelectedIndex = 0;
            }
        }
    }
}
