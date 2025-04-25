using Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces;
using Microsoft.Win32;
using System.IO;
using SystemConstants = Gamanet.C4.Client.Utils.Constants.Constants.System;

namespace Gamanet.C4.Client.Panels.DemoPanel.WPF.Windows.Services
{
    public class FileDialogService : IFileDialogService
    {
        public string? OpenFile(string filter, string? initialFilePath = null)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
            };

            if (!string.IsNullOrWhiteSpace(initialFilePath))
            {
                string? fileName = Path.GetFileName(initialFilePath);

                // File does not need to exist, it's only for pre-selection in filename text box of OpenFileDialog
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    dialog.FileName = fileName;
                }

                string? dirPath = Path.GetDirectoryName(initialFilePath);
                if (!string.IsNullOrWhiteSpace(dirPath) && Directory.Exists(dirPath))
                {
                    DirectoryInfo? directoryInfo = new(dirPath);

                    while (directoryInfo?.FullName.Length > SystemConstants.MAX_PATH
                            || directoryInfo?.Exists != true)
                    {
                        // climb up to parent directory as long as path is still to long
                        directoryInfo = directoryInfo?.Parent;
                    }
                    dialog.InitialDirectory = dirPath;
                }
            }

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
