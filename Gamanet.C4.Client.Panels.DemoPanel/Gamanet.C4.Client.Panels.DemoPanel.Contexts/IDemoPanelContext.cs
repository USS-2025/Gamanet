using Gamanet.C4.Client.Panels.DemoPanel.Entities;

namespace Gamanet.C4.Client.Panels.DemoPanel.Contexts
{
    public interface IDemoPanelContext : IAppContext
    {
        Task<IEnumerable<PersonEntity>> GetAllPersonsAsync();
    }
}
