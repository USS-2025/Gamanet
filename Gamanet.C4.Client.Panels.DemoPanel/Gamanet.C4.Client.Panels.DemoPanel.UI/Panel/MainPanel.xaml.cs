// If set, (Web) Application hosting with all its nice features will be used like in an ASP.NET app by default

using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Constants;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        private readonly IServiceProvider? _serviceProvider;
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
            var modelToAttach = _serviceProvider?.GetRequiredService<MainPanelViewModel>() ?? new MainPanelViewModel();
            _model = modelToAttach;

            string infoMsg = $"{nameof(MainPanel_Loaded)}:" +
                $" Attaching {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}...";
            Trace.TraceInformation(infoMsg);

            // Set it only once per MainPanel to have only one instance per MainPanel.
            // Getting model instance per service provider will cause constructor DI to work.

            // Furthermore, we have to ensure that no later data context change will override this data context
            // since data context will be derived from main window

            //this.RootContainer.DataContext = _model = _serviceProvider.GetRequiredService<MainPanelViewModel>();
            this.DataContext = modelToAttach;

            infoMsg = $"{nameof(MainPanel_Loaded)}: Attached {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}.";
            Trace.TraceInformation(infoMsg);

            if (this.DataContext is MainPanelViewModel model)
            {
                model.StatusText = infoMsg;


                // Only for a test:
                //model.LoadPersons().Wait();
                //model.LoadPersons();
                //await model.LoadPersons();
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    model.FillDesignData();
                }
            }
        }

        private void MainPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string infoMsg = $"{nameof(MainPanel_DataContextChanged)}: {e.OldValue ?? "null"} --> {e.NewValue ?? "null"}";
            Trace.TraceInformation(infoMsg);

            if (this.DataContext is not MainPanelViewModel)
            {
                // This could happen if parent's data context changed (MainWindow, 
                // so change it back to the model we want to use
                var modelToAttach = _serviceProvider?.GetRequiredService<MainPanelViewModel>() ?? new MainPanelViewModel();
                _model = modelToAttach;

                Trace.TraceInformation($"{nameof(MainPanel_DataContextChanged)}:" +
                    $" Attaching {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}...");

                // Set it only once per MainPanel to have only one instance per MainPanel.
                // Getting model instance per service provider will cause constructor DI to work.
                //this.RootContainer.DataContext = _model = _serviceProvider.GetRequiredService<MainPanelViewModel>();

                // Furthermore, we have to ensure that no later data context change will override this data context
                // since data context will be derived from main window
                this.DataContext = modelToAttach;

                infoMsg = $"{nameof(MainPanel_DataContextChanged)}: Attached {modelToAttach} to {nameof(DataContext)} of {nameof(MainPanel)}.";
                Trace.TraceInformation(infoMsg);

                if (this.DataContext is MainPanelViewModel model)
                {
                    model.StatusText = infoMsg;


                    // Only for a test:
                    //model.LoadPersons().Wait();
                    //model.LoadPersons();
                    //await model.LoadPersons();
                    if (DesignerProperties.GetIsInDesignMode(this))
                    {
                        model.FillDesignData();
                    }
                }
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

        private void CheckBoxSort_Click(object sender, RoutedEventArgs e)
        {
            if (_model == null)
            {
                return;
            }

            if (sender is not CheckBox checkBox)
            {
                return;
            }

            ListSortDirection? sortOrder = checkBox.IsChecked switch
            {
                true => (ListSortDirection?)ListSortDirection.Ascending,
                false => (ListSortDirection?)ListSortDirection.Descending,
                _ => null,// Default sort order: None (original from CSV)
            };

            switch (sender)
            {
                case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByName):
                    // Sort by name
                    _model.SortByPropertyNameToggle(nameof(PersonEntity.Name), sortOrder);
                    break;

                case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByCountry):
                    // Sort by name
                    _model.SortByPropertyNameToggle(nameof(PersonEntity.Country), sortOrder);
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

            await _model.LoadPersons(showFileDialog: true);
        }

        private void SetFilterByCountryInitialToAllCountries()
        {
            if (this.CountryFilterCombo.Items.Count > 0)
            {
                this.CountryFilterCombo.SelectedIndex = 0;
                // Yes, select "all countries" should always be the first item and not on 76. place :-)
                this.CountryFilterCombo.SelectedItem = FilterConstants.ALL_COUNTRIES;
                this.CountryFilterCombo.SelectedValue = FilterConstants.ALL_COUNTRIES;
            }
        }

        #region Only for some manual tests

        private void Button_ClearLists_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoRemoveAfterTestItemsControl.DataContext is MainPanelViewModel panelModel)
            {
                panelModel.ResetPeopleFillListProperties();
            }
        }

        private void ButtonTestInVM_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoRemoveAfterTestItemsControl.DataContext is MainPanelViewModel panelModel)
            {
                panelModel.FillDesignData();
                SetFilterByCountryInitialToAllCountries();
            }
        }

        private async void ButtonLoadCsv_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoRemoveAfterTestItemsControl.DataContext is MainPanelViewModel panelModel)
            {
                await panelModel.ReadCsvFile();
                SetFilterByCountryInitialToAllCountries();
            }
        }

        private void ButtonTestInCodeBehind_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be = ToDoRemoveAfterTestItemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty);

            object dataItem = be.DataItem;
            Trace.TraceInformation($"dataItem: {dataItem}");

            if (ToDoRemoveAfterTestItemsControl.ItemsSource is ICollection<PersonEntity> persCollection)
            {
                Trace.TraceInformation($"ToDoRemoveAfterTestItemsControl.DataContext: {ToDoRemoveAfterTestItemsControl.DataContext}");

                persCollection.Clear();

                foreach (var person in (List<PersonEntity>)[.. TestDataProvider.GetRandomPeople(10)])
                {
                    persCollection.Add(person);
                }
                SetFilterByCountryInitialToAllCountries();
            }
        }

        #endregion

    }
}
