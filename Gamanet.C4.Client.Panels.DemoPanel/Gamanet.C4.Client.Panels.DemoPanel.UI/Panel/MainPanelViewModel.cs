using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.MVVM;
using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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

        public int PersonsCount { get => this.Persons.Count;} 

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

            // ToDo: Comment in if working
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                this.ErrorText = "Design mode: Test-Error.";

                var testPeopleForDesignMode = new List<PersonEntity>
                    ( [
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
                for(int i = 4; i<100;i++)
                {
                    testPeopleForDesignMode.Add(new()
                    { 
                        Name= $"Test Person {i}", Country= $"Poland", Phone= $"012 345 {i:000}", Email= $"email.address{i:00}@mail.com"
                    });
                }

                UpdatePeopleList(testPeopleForDesignMode);
                this.CanLoadData = true;
            }
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


        public async Task LoadPersons()
        {
            if (_dpContext == null || !this.CanLoadData)
            {
                return;
            }

#if USE_APP_HOSTING
            if (_fileDlgService != null)
            {
                string initialFilePath = Path.Combine(Environment.CurrentDirectory, DEFAULT_CSV_FILEPATH_RELATIVE);

                try
                {
                    // Just for demonstration: This time using extension method
                    // which does not require explicit type conversion since it's a generic method:
                    /// <see cref="ServiceProviderServiceExtensions.GetRequiredService{T}(IServiceProvider)"/>

                    // Note: This works only if we registered this service as single instance!
                    var dataSource = _serviceProvider.GetRequiredService<IPersonDataSource>();

                    if (dataSource is IExcelPersonDataSource excelSource &&
                        !string.IsNullOrWhiteSpace(initialFilePath) && Path.Exists(initialFilePath))
                    {
                        string? selectedFilePath = _fileDlgService.OpenFile("CSV Files|*.csv|" +
                                                                            "Excel Worksheets|*.xls|*.xlsx", initialFilePath);

                        if (!string.IsNullOrWhiteSpace(selectedFilePath) && Path.Exists(selectedFilePath))
                        {
                            // File path for Excel reader
                            excelSource.ExcelFilePath = selectedFilePath;
                            // File path for this view model to show in view
                            this.ExcelFilePath = selectedFilePath;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error on resolving service '{nameof(IExcelPersonDataSource)}'" +
                                    $" (Did you forget to register in Startup / Bootstrapper?): {ex}");
                }
#endif
            }

            // put to separate task for error evaluation
            Task<IEnumerable<PersonEntity>> fetchPeopleTask = _dpContext.GetAllPersonsAsync();

            //await Task.Factory.StartNew(async () =>
            //{

            // Make a copy to RAM here if you are sure your data has limited data sets anyway
            var people = (await fetchPeopleTask).ToList();

            // Call Invoke() or InvokeAsync() to ensure that the code runs on the UI thread.
            // Attention: This is a blocking call, so be careful with long running tasks.

            // !!! ATTENTION !!!
            // Call it for whole operation and not after each iteration
            // since this causes too many Calling Context redirections and thus heavy loss of performance!

            try
            {
                await Dispatcher.CurrentDispatcher.InvokeAsync(callback: () =>
                {
                    try
                    {
                        Dispatcher.CurrentDispatcher.VerifyAccess();

                        this.ErrorText = string.Empty;

                        if (fetchPeopleTask.Status != TaskStatus.RanToCompletion)
                        {
                            this.ErrorText =
                                $"Error loading data: {fetchPeopleTask.Exception?.Message}";

                            return;
                        }

                        this.UpdatePeopleList(people);
                    }
                    catch (Exception ex)
                    {
                        this.ErrorText = $"Error loading data: {ex.Message}";
                    }
                    finally
                    {
                        // Anyway (if something went wrong or not, don't forget to make button visible again)
                        this.CanLoadData = true;
                    }

                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error on await Dispatcher.CurrentDispatcher.InvokeAsync: {ex}");
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
            // ToDo: Comment in if working
            //this.PersonsView?.DeferRefresh();
            this.Countries.Clear();
            this.Persons.Clear();
            OnPropertyChanged(nameof(this.PersonsCount));

            foreach (var person in people)
            {
                this.Persons.Add(person);
            }
            OnPropertyChanged(nameof(this.PersonsCount));

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
                // Remove only affected sorter but not any other since we are able to sort by multiple properties ".ThenBy(...)". 
                /// <seealso cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey}?)"/>
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
                // Remove only affected sorter but not any other since we are able to sort by multiple properties ".ThenBy(...)". 
                /// <seealso cref="Enumerable.ThenByDescending{TSource, TKey}(IOrderedEnumerable{TSource}, Func{TSource, TKey}, IComparer{TKey}?)"/>
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
    }
}
