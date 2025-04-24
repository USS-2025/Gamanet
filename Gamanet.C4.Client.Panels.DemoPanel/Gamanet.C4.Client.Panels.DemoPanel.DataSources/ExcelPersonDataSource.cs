using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using MiniExcelLibs;

namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources
{
    public class ExcelPersonDataSource: IExcelPersonDataSource
    {
#pragma warning disable S125
        // Define a case-insensitive regular expression for repeated words.
        //// If no (optional) last name, the whole entry will be considers as first name
        //static readonly Regex REGEX_NAME = new Regex(
        //                    @"^\s*(?<firstname>\w+)\s+(?<lastname>\w+)?\r*$"
        //                        , RegexOptions.Compiled | RegexOptions.IgnoreCase);
#pragma warning restore S125

        private const string DEFAULT_CSV_FILEPATH_RELATIVE = @".\Assets\TestData\PersonsDemo.csv";

        public string ExcelFilePath { get; set; }
        
        public ExcelPersonDataSource()
        {
            this.ExcelFilePath = DEFAULT_CSV_FILEPATH_RELATIVE;
        }

        public async Task<IEnumerable<PersonEntity>> LoadPersonsAsync()
        {
            if(string.IsNullOrWhiteSpace(this.ExcelFilePath))
            {
                return [];
            }

            string testPath = Path.Combine( Environment.CurrentDirectory, this.ExcelFilePath);

            if (!File.Exists(testPath))
            {
                return [];
            }

            string ext = Path.GetExtension(testPath);

            var excelType = ext switch
            {
                ".xls" or ".xlsx" => ExcelType.XLSX,
                ".csv" => ExcelType.CSV,
                _ => ExcelType.UNKNOWN,
            };

            IEnumerable<PersonEntity> list = await MiniExcel.QueryAsync<PersonEntity>(testPath, sheetName: "PersonsDemo", excelType);

            return list;
        }
    }
}
