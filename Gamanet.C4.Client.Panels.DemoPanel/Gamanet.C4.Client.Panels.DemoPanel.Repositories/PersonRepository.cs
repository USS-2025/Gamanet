using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;

namespace Gamanet.C4.Client.Panels.DemoPanel.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IPersonDataSource? _dataSource;


        public PersonRepository() 
        {
#if USE_APP_HOSTING
#else
            _dataSource = new ExcelPersonDataSource();
#endif
        }

        /// <summary>
        /// 
        /// Constructor for DI (Dependency Injection).
        /// 
        /// Will be called on: 
        /// App.AppHost.Services.GetService(typeof(IPersonRepository)) as IPersonRepository;
        /// or:
        /// App.AppHost.Services.GetService(typeof(PersonRepository)) as PersonRepository;
        /// 
        /// </summary>
        public PersonRepository(IPersonDataSource dataSource)
            :this()
        {
            _dataSource = dataSource;
        }

        public async Task<IEnumerable<PersonEntity>> GetAllPersonsAsync()
        {
            if(_dataSource==null)
            {
                return [];
            }

            return await _dataSource.LoadPersonsAsync();
        }
    }
}
