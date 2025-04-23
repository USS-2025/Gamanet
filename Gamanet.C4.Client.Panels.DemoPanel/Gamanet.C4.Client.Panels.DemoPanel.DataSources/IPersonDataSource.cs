using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources
{
    public interface IPersonDataSource
    {
        public Task<IEnumerable<PersonEntity>> LoadPersonsAsync(string path);
    }
}
