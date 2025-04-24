using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter, string? initialFilePath = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                FileName = initialFilePath,
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
