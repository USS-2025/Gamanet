using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.MVVM;
using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Panel
{
    internal class MainPanelViewModel : ViewModelBase
    {
        private const string DEFAULT_CSV_FILEPATH_RELATIVE = @".\Assets\TestData\PersonsDemo.csv";

        private readonly IServiceProvider? _serviceProvider;

        private readonly IDemoPanelContext? _dpContext;
        private readonly IFileDialogService? _fileDlgService;

        private string? _excelFilePath;

        public string? ExcelFilePath
        {
            get => _excelFilePath;
            set
            {
                if (value == _excelFilePath)
                {
                    return;
                }

                _excelFilePath = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Countries { get; } = [];

        /// <summary>
        /// Correct plural name of 'Person' is actually 'People'.
        /// But this violates convention, to just put a 's' suffix to according collection property name.
        /// </summary>
        public ObservableCollection<PersonEntity> Persons { get; } = [];

        public ICollectionView? PersonsView { get; }

        private bool _canLoadData = true;

        public bool CanLoadData
        {
            get { return _canLoadData; }
            set
            {
                if (value == _canLoadData)
                {
                    return;
                }

                _canLoadData = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Parameterless constructor, Design Time cannot use any DI (Dependency Injection) here.
        /// </summary>
        public MainPanelViewModel()
        {
            _serviceProvider = App.ServiceProvider;

            // Just for demonstration: Using method declared in System namespace:
            /// <see cref="IServiceProvider.GetService(Type)"/> 
            _dpContext = _serviceProvider?.GetRequiredService<IDemoPanelContext>() ?? new _DemoPanelContext();
            _fileDlgService = _serviceProvider?.GetRequiredService<IFileDialogService>() ?? new FileDialogService();

            // Create view for filtering/sorting
            this.PersonsView = CollectionViewSource.GetDefaultView(this.Persons);

            this.FillDesignData();
        }

        /// <summary>
        /// 
        /// Constructor for DI (Dependency Injection).
        /// 
        /// Will be called on: 
        /// App.AppHost.Services.GetService(typeof(IViewModel)) as IViewModel;
        /// or:
        /// App.AppHost.Services.GetService(typeof(MainPanelViewModel)) as MainPanelViewModel;
        /// 
        /// </summary>
        public MainPanelViewModel(IDemoPanelContext dpContext)
            : this()
        {
            _dpContext = dpContext;
        }

        public async Task LoadPersons(bool showFileDialog = true)
        {

            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: {this}: {nameof(LoadPersons)}: CALL {nameof(LoadPersonsAsync)}()...");
            // from here on not in UI Thread anymore
            await LoadPersonsAsync(showFileDialog);
        }

        private string? GetCsvOrExcelFilePath(bool showFileDialog = true)
        {
            string? selectedFilePath;

            string initialFilePath = Path.Combine(
                // trimming necessary because Path.Combine() has strange behavior if something is not perfect
                Environment.CurrentDirectory.TrimEnd('\\'),
                DEFAULT_CSV_FILEPATH_RELATIVE.TrimStart('\\', '.'));

            if (_fileDlgService != null)
            {
                try
                {
                    // Just for demonstration: This time using extension method
                    // which does not require explicit type conversion since it's a generic method:
                    /// <see cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>

                    // Skip opening dialog for fast testing purposes(everything should be configurable)

                    selectedFilePath = showFileDialog ?
                                            _fileDlgService.OpenFile("CSV Files|*.csv|" +
                                                                    "Excel Worksheets (*.xls, *.xlsx)|*.xls;*.xlsx", initialFilePath)
                                            : initialFilePath;

                    // Note: This works only if we registered this service as single instance!
                    var dataSource = _serviceProvider?.GetRequiredService<IPersonDataSource>() ?? new ExcelPersonDataSource();

                    if (dataSource is IExcelPersonDataSource excelDataSource)
                    {
                        // File path for Excel reader
                        excelDataSource.ExcelFilePath = selectedFilePath;
                    }
                }
                catch (Exception ex)
                {

                    string errMsg = $"Error on opening {nameof(Microsoft.Win32.OpenFileDialog)} for CSV/Excel: {ex}";
                    Trace.TraceError($"[{Environment.CurrentManagedThreadId:000}]: {errMsg}");

                    selectedFilePath = initialFilePath;

                    this.CanLoadData = true;
                    this.ErrorText = errMsg;
                }
            }
            else
            {
                selectedFilePath = initialFilePath;
            }

            // File path for this view model to show in view
            this.ExcelFilePath = selectedFilePath;

            return selectedFilePath;
        }

        /// <summary>
        /// Loads all people from selected CSV or Excel.
        /// </summary>
        /// <param name="showFileDialog">
        /// If false, <see cref="DEFAULT_CSV_FILEPATH_RELATIVE"/> will be used.
        /// (More for fast testing purposes).
        /// </param>
        public async Task LoadPersonsAsync(bool showFileDialog = true)
        {
            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: {this}: {nameof(LoadPersonsAsync)} called.");

            if (_dpContext == null || !this.CanLoadData)
            {
                return;
            }

            // Still in UI thread --> write data bound properties here
            this.CanLoadData = false;

            this.ResetPeopleFillListProperties();

            string? selectedFilePath = this.GetCsvOrExcelFilePath(showFileDialog);

            if (string.IsNullOrWhiteSpace(selectedFilePath))
            {
                this.CanLoadData = true;
                return;
            }

            List<PersonEntity>? people;
            Exception? fetchPeopleListException;

            // Execute query by .ToList()
            // Make a copy to RAM here if you are sure your data has limited data sets anyway
            // If not, please us pagination mechanism
            try
            {
                people = [.. await _dpContext.GetAllPersonsAsync()];
                fetchPeopleListException = null;
            }
            catch (Exception ex)
            {
                string errMsg = $"Error on reading {(string.IsNullOrWhiteSpace(selectedFilePath) ?
                                                    Path.GetFileName(selectedFilePath) :
                                                    "CSV or Excel")}: {ex}";
                Trace.TraceError($"[{Environment.CurrentManagedThreadId:000}]: {errMsg}");

                try
                {
                    // Try at least to enable button again for next time (we are not in UI thread here).
                    await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        this.CanLoadData = true;
                        this.ErrorText = errMsg;
                    }, DispatcherPriority.DataBind);
                }
                catch
                {
                    Trace.TraceError($"[{Environment.CurrentManagedThreadId:000}]: Error on Dispatcher Invoke: {ex}");
                }

                return;
            }

            // Call InvokeAsync() to ensure that the code inside this callback delegate runs on the UI thread.
            // Attention: This is a blocking call, so be careful with long running tasks.

            // !!! ATTENTION !!!
            // Call it for whole operation once and not after each iteration
            // since this causes too many Calling Context redirections and thus heavy loss of performance!
            try
            {
                Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: Dispatcher.CurrentDispatcher.InvokeAsync...");
                await Dispatcher.CurrentDispatcher.InvokeAsync(callback: () =>
                {
                    Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: Dispatcher.CurrentDispatcher.VerifyAccess()...");
                    Dispatcher.CurrentDispatcher.VerifyAccess();
                    Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: Dispatcher.CurrentDispatcher.VerifyAccess() OK.");

                    this.ErrorText = string.Empty;

                    // at first check if something went wrong before with fetching data
                    if (fetchPeopleListException != null)
                    {
                        string errMsg = $"[{Environment.CurrentManagedThreadId:000}]: Error loading data: {fetchPeopleListException.Message}";
                        Trace.TraceError(errMsg);
                        this.ErrorText = errMsg;

                        // enable button again for next use / attempt
                        this.CanLoadData = true;
                    }
                    else
                    {
                        // go on here if no exception before
                        // but even if no exception (e.g. 3rd party library doesn't throw any exception on error),
                        // check nevertheless if people list is null or empty
                        if (people?.Count > 0)
                        {
                            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: UpdatePeopleList...");
                            this.UpdatePeopleList(people);
                            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: UpdatePeopleList finished.");
                        }
                        else
                        {
                            string wrnMsg = $"No people could be found in list.";
                            this.ErrorText = wrnMsg;
                            Trace.TraceWarning($"[{Environment.CurrentManagedThreadId:000}]: {wrnMsg}");
                        }

                        // Anyway (if something went wrong or not, don't forget to make button visible again)
                        this.CanLoadData = true;
                    }

                }, DispatcherPriority.Background);
                Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: Dispatcher.CurrentDispatcher.InvokeAsync OK.");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"[{Environment.CurrentManagedThreadId:000}]: Error on await Dispatcher.CurrentDispatcher.InvokeAsync: {ex}");
            }
            finally
            {
                // Anyway (if something went wrong or not, don't forget to make button visible again)
                this.CanLoadData = true;
            }
            //}
            //);
        }

        /// <summary>
        /// Synchronous Helper Method, never call it in UI Thread with too many items!
        /// </summary>
        /// <param name="people"></param>
        public void UpdatePeopleList(IEnumerable<PersonEntity> people)
        {
            this.CanLoadData = false;

            this.ResetPeopleFillListProperties();

            foreach (var person in people)
            {
                this.Persons.Add(person);
            }

            this.StatusText = $"{this}: Added {this.Persons.Count} people to collection.";
            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: {this}: Added {this.Persons.Count} people to collection.");

            // Don't forget leading separated value for no filtering (all countries)
            var countries = new string[] { Constants.FilterConstants.ALL_COUNTRIES }
                        .Concat(people
                            .Where(p => !string.IsNullOrWhiteSpace(p.Country))
                            .Select(p => p.Country!)
                            .Distinct()
                            .Order());

            foreach (var country in countries)
            {
                this.Countries.Add(country);
            }

            this.CanLoadData = true;
        }

        public void ResetPeopleFillListProperties()
        {
            this.Countries.Clear();
            this.Persons.Clear();
        }

        /// <summary>
        /// Sorts the <see cref="PersonsView"/> of underlying <see cref="Persons"/>
        /// by given name and given order.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="sortOrder">if null, all sortings will be removed.</param>
        public void SortByPropertyNameToggle(string propertyName, ListSortDirection? sortOrder)
        {
            if (this.PersonsView == null)
            {
                return;
            }

            SortDescription? sortingByPropertyName;

            switch (propertyName)
            {
                case nameof(PersonEntity.Name):
                    sortingByPropertyName = this.PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Name));
                    break;

                case nameof(PersonEntity.Country):
                    sortingByPropertyName = this.PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Country));
                    break;

                // add more properties in future here if needed

                default:
                    sortingByPropertyName = null;
                    break;
            }

            if (sortingByPropertyName == null)
            {
                return;
            }

            using (this.PersonsView.DeferRefresh())
            {
                // at first remove all existing sort descriptions anyway
                // Remove only affected sorter but not any other since we are able to sort by multiple properties ".ThenBy(...)". 
                /// <seealso cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey}?)"/>
                this.PersonsView?.SortDescriptions.Remove(sortingByPropertyName.Value);

                switch (sortOrder)
                {
                    case ListSortDirection.Ascending:
                        // if never sorted or currently sorted descending, sort ascending
                        this.PersonsView?.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Ascending));
                        break;
                    case ListSortDirection.Descending:
                        // if currently sorted ascending, sort descending
                        this.PersonsView?.SortDescriptions.Add(new SortDescription(propertyName, ListSortDirection.Descending));
                        break;
                    default:
                        // Do nothing, we already removed affected sort description before
                        break;
                }
            }

            this.PersonsView?.Refresh();
        }

        public void FilterByCountry(string selectedCountry)
        {
            using (this.PersonsView?.DeferRefresh())
            {
                if (this.PersonsView == null)
                {
                    return;
                }

                // if empty or all countries selected, don't filter out anything
                if (string.IsNullOrWhiteSpace(selectedCountry)
                    || string.Equals(selectedCountry, Constants.FilterConstants.ALL_COUNTRIES, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.PersonsView.Filter = null;
                }
                else
                {
                    this.PersonsView.Filter = obj =>
                    {
                        // don't filter out, if:
                        // 1. Our object is not of type PersonEntity for some reason
                        if (obj is not PersonEntity person)
                        {
                            return true;
                        }

                        // 2. country was null, empty or whitespace
                        // (show this person to user and let him know that his/her country was empty in CSV or Excel
                        if (string.IsNullOrWhiteSpace(person.Country))
                        {
                            return true;
                        }

                        // if everything is OK, filter by country without heavy string comparison performance loss
                        return string.Equals(person.Country, selectedCountry, StringComparison.InvariantCultureIgnoreCase);
                    };
                }
            }
            PersonsView.Refresh();
        }


        #region Only for some manual tests or showing design data

        public void FillDesignData()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ErrorText = "Only for test in Design mode: Test-Error.";
            }

            UpdatePeopleList(TestDataProvider.GetTestPeople(50));

            this.CanLoadData = true;
        }

        internal async Task ReadCsvFile()
        {
            await LoadPersons(showFileDialog: false);
        }

        #endregion
    }
}
