using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? OpenFile(string filter, string? initialFilePath = null);
    }
}
