using AutoMapper;
using Gamanet.C4.Client.Panels.DemoPanel.DataSources.Interfaces;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using MiniExcelLibs;

namespace Gamanet.C4.Client.Panels.DemoPanel.DataSources
{
    public class ExcelPersonDataSource : IExcelPersonDataSource
    {
#pragma warning disable S125
        // Define a case-insensitive regular expression for repeated words.
        //// If no (optional) last name, the whole entry will be considers as first name
        //static readonly Regex REGEX_NAME = new Regex(
        //                    @"^\s*(?<firstname>\w+)\s+(?<lastname>\w+)?\r*$"
        //                        , RegexOptions.Compiled | RegexOptions.IgnoreCase);
#pragma warning restore S125

        private const string DEFAULT_CSV_FILEPATH_RELATIVE = @".\Assets\TestData\PersonsDemo.csv";

        public string? ExcelFilePath { get; set; }

        public ExcelPersonDataSource()
        {
            this.ExcelFilePath = DEFAULT_CSV_FILEPATH_RELATIVE;
        }

        public async Task<IEnumerable<PersonEntity>> LoadPersonsAsync()
        {
            if (string.IsNullOrWhiteSpace(this.ExcelFilePath))
            {
                return [];
            }

            string testPath = Path.Combine(Environment.CurrentDirectory, this.ExcelFilePath);

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

            IEnumerable<PersonDto> dtoList = await MiniExcel.QueryAsync<PersonDto>(testPath, sheetName: "PersonsDemo", excelType);

            // Only for a later test: simulate long work here to test reaction of UI
            /// await Task.Delay(5000);
            ///////////////////////////////////////////////////////////////////

            List<PersonEntity> peopleList = new(dtoList.Count());

            // Configure AutoMapper
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<PersonDto, PersonEntity>()
                    // This we only need to do as long we have case-sensitive reading
                    // ()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                    .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.country))
                    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.phone))
                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                // add more here if you need
                );


            var mapper = configuration.CreateMapper();

            foreach (PersonDto dto in dtoList)
            {
                peopleList.Add(mapper.Map<PersonEntity>(dto));
            }

            return peopleList;
        }
    }
}
