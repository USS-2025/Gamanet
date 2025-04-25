namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces
{
    public interface IExcelPersonDataSource: IPersonDataSource
    {
        public string? ExcelFilePath { get; set; }
    }
}
