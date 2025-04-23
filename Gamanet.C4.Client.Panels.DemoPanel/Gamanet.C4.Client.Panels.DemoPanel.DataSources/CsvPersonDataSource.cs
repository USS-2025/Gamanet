using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using Microsoft.VisualBasic.FileIO;
using MiniExcelLibs;

using System.Text.RegularExpressions;

namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources
{
    public class CsvPersonDataSource: IPersonDataSource
    {
        private const string DEFAULT_CSV_FILEPATH_RELATIVE = @".\Assets\TestData\PersonsDemo.csv";

#pragma warning disable S125
        // Define a case-insensitive regular expression for repeated words.
        //// If no (optional) last name, the whole entry will be considers as first name
        //static readonly Regex REGEX_NAME = new Regex(
        //                    @"^\s*(?<firstname>\w+)\s+(?<lastname>\w+)?\r*$"
        //                        , RegexOptions.Compiled | RegexOptions.IgnoreCase);
#pragma warning restore S125

        public async Task<IEnumerable<PersonEntity>> LoadPersonsAsync(string path = DEFAULT_CSV_FILEPATH_RELATIVE)
        {
            string testPath = Path.Combine( Environment.CurrentDirectory, path);

            if (!File.Exists(testPath))
            {
                return [];
            }

            string ext = Path.GetExtension(testPath);

            ExcelType excelType;

            switch(ext)
            {
                case ".xls":
                case ".xlsx":
                    excelType = ExcelType.XLSX;
                    break;
                case ".csv":
                    excelType = ExcelType.CSV;
                    break;
                default:
                    excelType = ExcelType.UNKNOWN;
                    break;
            }

            IEnumerable<PersonEntity> list = await MiniExcel.QueryAsync<PersonEntity>(testPath, sheetName: "PersonsDemo", excelType);

            return list;
        }
    }
}
