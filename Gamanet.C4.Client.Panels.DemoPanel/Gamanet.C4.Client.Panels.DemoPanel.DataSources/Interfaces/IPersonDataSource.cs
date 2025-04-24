using Gamanet.C4.Client.Panels.DemoPanel.Entities;

namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces
{
    public interface IPersonDataSource
    {
        /// <summary>
        /// Generic method for loading from a configured database.
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<PersonEntity>> LoadPersonsAsync();
    }
}
