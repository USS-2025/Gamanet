﻿using Gamanet.C4.Client.Panels.DemoPanel.Entities;

namespace Gamanet.C4.Client.Panels.DemoPanel.Repositories
{
    public interface IPersonRepository
    {
        Task<IEnumerable<PersonEntity>> GetAllPersonsAsync();
    }
}
