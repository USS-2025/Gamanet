using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Gamanet.C4.Client.Panels.DemoPanel.Repositories;
using Gamanet.C4.Client.Panels.DemoPanel.Contexts;
using System;

namespace Gamanet.C4.Client.Panels.DemoPanel.Contexts
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public class _DemoPanelContext : IDemoPanelContext
    {
        private readonly IPersonRepository? _personRepo;

        public _DemoPanelContext()
        {
            _personRepo = new PersonRepository();
        }

        /// <summary>
        /// 
        /// Constructor for DI (Dependency Injection).
        /// 
        /// Will be called on: 
        /// App.AppHost.Services.GetService(typeof(IDemoPanelContext)) as IDemoPanelContext;
        /// or:
        /// App.AppHost.Services.GetService(typeof(_DemoPanelContext)) as _DemoPanelContext;
        /// 
        /// </summary>
        public _DemoPanelContext(IPersonRepository personRepo)
            : this()
        {
            _personRepo = personRepo;
        }

        public async Task<IEnumerable<PersonEntity>> GetAllPersonsAsync()
        {
            if (_personRepo == null)
            {
                return [];
            }

            // Simulate long work for testing
            await Task.Delay(TimeSpan.FromSeconds(3));

            return await _personRepo.GetAllPersonsAsync();
        }
    }
}
