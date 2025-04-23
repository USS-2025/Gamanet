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

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel
{
    /// <summary>
    /// Interaction logic for MainPanel.xaml
    /// </summary>
    public partial class MainPanel : UserControl
    {
        //private readonly MainPanelViewModel? _viewModel;

        public MainPanel()
        {
            InitializeComponent();

            //_viewModel = (MainPanelViewModel)DataContext;
            var context = this.DataContext;

            this.Loaded += this.MainPanel_Loaded; ;   
        }

        private void MainPanel_Loaded(object sender, RoutedEventArgs e) 
        {
            //_viewModel = (MainPanelViewModel)DataContext;
            var context = this.DataContext;
        }

        private void CountryFilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is MainPanelViewModel viewModel
                && sender == this.CountryFilterCombo)
            {
                viewModel.FilterByCountry((string)CountryFilterCombo.SelectedItem);
            }

        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainPanelViewModel viewModel
                && sender is Control control)
            {
                switch (sender)
                {
                    case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByName):
                        // Sort by name
                        viewModel.SortByNameToggle();
                        break;

                    case CheckBox _ when ReferenceEquals(sender, this.CheckboxSortByName):
                        // Sort by name
                        viewModel.SortByNameToggle();
                        break;

                    default:
                        // Handle other controls or log warning
                        break;
                }
            }
        }
    }
}
