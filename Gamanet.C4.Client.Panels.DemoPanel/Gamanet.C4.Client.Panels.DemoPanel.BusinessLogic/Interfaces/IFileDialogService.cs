namespace Gamanet.C4.Client.Panels.DemoPanel.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? OpenFile(string filter, string? initialFilePath = null);
    }
}
