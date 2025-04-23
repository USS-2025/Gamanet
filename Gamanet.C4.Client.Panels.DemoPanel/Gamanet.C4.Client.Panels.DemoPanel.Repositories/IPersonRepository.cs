using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamanet.C4.Client.Panels.DemoPanel.Repositories
{
    public interface IPersonRepository
    {
        private readonly CsvPersonDataSource _dataSource;

        Task GetAllPersonsAsync();
    }
}
