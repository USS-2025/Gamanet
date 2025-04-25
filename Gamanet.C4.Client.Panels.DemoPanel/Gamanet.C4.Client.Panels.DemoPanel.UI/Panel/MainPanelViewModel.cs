using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.MVVM;
using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
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

        private readonly IServiceProvider _serviceProvider;

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
            _dpContext = _serviceProvider.GetService(typeof(IDemoPanelContext)) as IDemoPanelContext;
            _fileDlgService = _serviceProvider.GetService(typeof(IFileDialogService)) as IFileDialogService;

            // Create view for filtering/sorting
            this.PersonsView = CollectionViewSource.GetDefaultView(this.Persons);

            this.FillDesignData();
        }

        public void FillDesignData()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ErrorText = "Design mode: Test-Error.";
            }

            var testPeopleForDesignMode = new List<PersonEntity>
                ([
                    new() { Name = "Test Person 1", Country = "Poland", Phone= "+48 123 456 789", Email = "testperson1@poczta.pl"},
                        new() { Name = "Test Person 2", Country = "Poland", Phone= "+48 456 789 123", Email = "testperson2@poczta.pl"},
                        new() { Name = "Test Person 3", Country = "Germany", Phone= "+49 789 123 456", Email = "Test: very_very_very_very_" +
                                                                                                       "very_very_very_very_" +
                                                                                                       "very_very_very_very_" +
                                                                                                       "very_very_very_very_" +
                                                                                                       "very_very_very_very_" +
                                                                                                       "long_email_address_testperson3@mail.de"},
                ]);

            // Now for testing many items (scroll bar visibilities, limiting size of items control etc.)
            for (int i = 4; i < 50; i++)
            {
                testPeopleForDesignMode.Add(new()
                {
                    Name = $"Test Person {i}",
                    Country = $"Poland",
                    Phone = $"012 345 {i:000}",
                    Email = $"email.address{i:00}@mail.com"
                });
            }

            UpdatePeopleList(testPeopleForDesignMode);
            this.CanLoadData = true;
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
            // Still in UI thread --> write data bound properties here
            this.ResetPeopleFillListProperties();

            Trace.TraceInformation($"[{Environment.CurrentManagedThreadId:000}]: {this}: {nameof(LoadPersons)}: CALL {nameof(LoadPersonsAsync)}()...");
            // from here on not in UI Thread anymore
            await LoadPersonsAsync(showFileDialog);
        }


        /// <summary>
        /// 
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

            string? selectedFilePath;

            if (_fileDlgService != null)
            {
                string initialFilePath = Path.Combine(
                    // trimming necessary because Path.Combine() has strange behavior if something is not perfect
                    Environment.CurrentDirectory.TrimEnd('\\'),
                    DEFAULT_CSV_FILEPATH_RELATIVE.TrimStart('\\', '.'));

                try
                {
                    // Just for demonstration: This time using extension method
                    // which does not require explicit type conversion since it's a generic method:
                    /// <see cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>

                    // Note: This works only if we registered this service as single instance!
                    var dataSource = _serviceProvider.GetRequiredService<IPersonDataSource>();

                    if (dataSource is IExcelPersonDataSource excelSource)
                    {
                        // Skip opening dialog for fast testing purposes (everything should be configurable)
                        selectedFilePath = showFileDialog ?
                                                _fileDlgService.OpenFile("CSV Files|*.csv|" +
                                                                        "Excel Worksheets (*.xls, *.xlsx)|*.xls;*.xlsx", initialFilePath)
                                                : initialFilePath;

                        if (!string.IsNullOrWhiteSpace(selectedFilePath) && Path.Exists(selectedFilePath))
                        {
                            // File path for Excel reader
                            excelSource.ExcelFilePath = selectedFilePath;
                            // File path for this view model to show in view
                            this.ExcelFilePath = selectedFilePath;
                        }
                    }
                    else
                    {
                        selectedFilePath = null;
                    }
                }
                catch (Exception ex)
                {

                    string errMsg = $"Error on opening {nameof(Microsoft.Win32.OpenFileDialog)} for CSV/Excel: {ex}";
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
            }
            else
            {
                selectedFilePath = null;
            }

            // put to separate task for error evaluation
            //Task<IEnumerable<PersonEntity>> fetchPeopleTask = _dpContext.GetAllPersonsAsync();

            //await Task.Factory.StartNew(async () =>
            //{

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

            // Call Invoke() or InvokeAsync() to ensure that the code runs on the UI thread.
            // Attention: This is a blocking call, so be careful with long running tasks.

            // !!! ATTENTION !!!
            // Call it for whole operation and not after each iteration
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

        private void ResetPeopleFillListProperties()
        {
            this.CanLoadData = false;
            this.Countries.Clear();
            this.Persons.Clear();
        }

        public void SortByNameToggle()
        {
            if (PersonsView == null)
            {
                return;
            }

            SortDescription? nameSorting = PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Name));

            using (this.PersonsView?.DeferRefresh())
            {
                if (nameSorting.HasValue)
                {
                    // Remove only affected sorter but not any other since we are able to sort by multiple properties ".ThenBy(...)". 
                    /// <seealso cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey}?)"/>
                    PersonsView?.SortDescriptions.Remove(nameSorting.Value);
                }

                if (nameSorting == null || nameSorting.GetValueOrDefault().Direction == ListSortDirection.Descending)
                {
                    // if never sorted or currently sorted descending, sort ascending
                    PersonsView.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Name), ListSortDirection.Ascending));
                }
                else
                {
                    // if currently sorted ascending, sort descending
                    PersonsView?.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Name), ListSortDirection.Descending));
                }
            }

            this.PersonsView?.Refresh();
        }

        public void SortByCountryToggle()
        {
            if (PersonsView == null)
            {
                return;
            }

            SortDescription? countrySorting = PersonsView.SortDescriptions.FirstOrDefault(s => s.PropertyName == nameof(PersonEntity.Country));

            using (this.PersonsView?.DeferRefresh())
            {
                if (countrySorting.HasValue)
                {
                    // Remove only affected sorter but not any other since we are able to sort by multiple properties ".ThenBy(...)". 
                    /// <seealso cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey}?)"/>
                    PersonsView?.SortDescriptions.Remove(countrySorting.Value);
                }

                if (countrySorting == null || countrySorting.GetValueOrDefault().Direction == ListSortDirection.Descending)
                {
                    // if never sorted or currently sorted descending, sort ascending
                    PersonsView?.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Country), ListSortDirection.Ascending));
                }
                else
                {
                    // if currently sorted ascending, sort descending
                    PersonsView?.SortDescriptions.Add(new SortDescription(nameof(PersonEntity.Country), ListSortDirection.Descending));
                }
            }

            this.PersonsView?.Refresh();
        }

        public void FilterByCountry(string country)
        {
            // ToDo: Comment in if working
            using (this.PersonsView?.DeferRefresh())
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
            }
            PersonsView.Refresh();
        }
    }
}
