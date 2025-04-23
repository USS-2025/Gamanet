using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Model;
using Gamanet.C4.Client.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel
{
    internal class MainPanelViewModel : PropertyChangedBase
    {
        private readonly IDemoPanelContext? _dpContext;

        public ObservableCollection<string> Countries { get; } = [];

        /// <summary>
        /// Correct plural name of 'Person' is actually 'People'.
        /// But this violates convention, to just put a 's' suffix to according collection property name.
        /// </summary>
        public ObservableCollection<PersonEntity> Persons { get; } = [];

        public ICollectionView? PersonsView { get; }

        public MainPanelViewModel()
        {

        }

        public MainPanelViewModel(IDemoPanelContext dpContext)
            :this()
        {
            _dpContext = dpContext;

            // Source collection
            var people = _dpContext.PersonRepo.Persons;

            // Create view for filtering/sorting
            PersonsView = CollectionViewSource.GetDefaultView(people);


            // Don't forget leading separated value for no filtering (all countries)
            Countries = [.. new string[] { Constants.FilterConstants.ALL_COUNTRIES }
                            .Concat(people
                                .Where(p => !string.IsNullOrWhiteSpace(p.Country))
                                .Select(p => p.Country!)
                                .Distinct()
                                .Order())];
        }

        public async Task LoadDataAsync()
        {
            this.Persons.Clear();

            var list = await _dataSource.LoadPersonsAsync();

            foreach (var p in list)
            {
                Persons.Add(p);
            }
        }


        public void SortByNameToggle()
        {
            if (PersonsView == null)
            {
                return;
            }

            SortDescription? nameSorting = PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Name));

            if (nameSorting.HasValue)
            {
                PersonsView.SortDescriptions.Remove(nameSorting.Value);
            }

            if (nameSorting == null || nameSorting.GetValueOrDefault().Direction == ListSortDirection.Descending)
            {
                // if never sorted or currently sorted descending, sort ascending
                PersonsView.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Name), ListSortDirection.Ascending));
            }
            else
            {
                // if currently sorted ascending, sort descending
                PersonsView.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Name), ListSortDirection.Descending));
            }
        }

        public void SortByCountryToggle()
        {
            if (PersonsView == null)
            {
                return; 
            }

            SortDescription? countrySorting = PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Country));

            if (countrySorting.HasValue)
            {
                PersonsView.SortDescriptions.Remove(countrySorting.Value);
            }

            if (countrySorting == null || countrySorting.GetValueOrDefault().Direction == ListSortDirection.Descending)
            {
                // if never sorted or currently sorted descending, sort ascending
                PersonsView.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Country), ListSortDirection.Ascending));
            }
            else
            {
                // if currently sorted ascending, sort descending
                PersonsView.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Country), ListSortDirection.Descending));
            }
        }

        public void FilterByCountry(string country)
        {
            if (PersonsView == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(country))
            {
                PersonsView.Filter = null;
            }
            else
            {
                PersonsView.Filter = o => ((PersonEntity)o).Country == country;
            }

            PersonsView.Refresh();
        }

        /// <summary>
        /// Only to show type of model in UI to check if data binding works
        /// </summary>
        /// <returns></returns>
        public override string ToString() => GetType().Name;
    }
}
