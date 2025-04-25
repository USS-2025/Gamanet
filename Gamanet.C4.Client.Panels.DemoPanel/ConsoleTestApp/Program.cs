using AutoMapper;
using Gamanet.C4.Client.Panels.DemoPanel.Entities;
using MiniExcelLibs;

public class PersonDto
{
#pragma warning disable IDE1006
    public string? name { get; set; }
    public string? country { get; set; }
    public string? address { get; set; }
    public string? postalZip { get; set; }
    public string? email { get; set; }
    public string? phone { get; set; }

#pragma warning disable IDE1006
}


internal class Program
{
    private const string DEFAULT_CSV_FILEPATH_RELATIVE = @".\Assets\TestData\PersonsDemo.csv";

    [STAThread] // GANZ WICHTIG für Dialoge!
    private static void Main(string[] args)
    {
        try
        {
            string currentDir = @"D:\Data\Projects\Repos\GitHub\Gamanet\Gamanet.C4.Client.Panels.DemoPanel\Gamanet.C4.Client.Panels.DemoPanel.UI\";

            string initialFilePath = Path.Combine(
                    currentDir.TrimEnd('\\'),
                    DEFAULT_CSV_FILEPATH_RELATIVE.TrimStart('\\', '.'));

            string initDir = Path.GetDirectoryName(initialFilePath);

            var config = new MiniExcelLibs.Csv.CsvConfiguration()
            {
                Seperator = ','
            };

            IEnumerable<PersonDto> dtoList = MiniExcel.Query<PersonDto>(initialFilePath, sheetName: "PersonsDemo", ExcelType.CSV, startCell: default, config);

            Console.WriteLine($"{dtoList.Count()} Eintreage eingelesen.");
            List<PersonEntity> peopleList = new List<PersonEntity>(dtoList.Count());

            // Configure AutoMapper
            var configuration = new MapperConfiguration(cfg =>
              cfg.CreateMap<PersonDto, PersonEntity>()
                // This we only need to do as long we have case-sensitive reading
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.country))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.phone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                );
            // add more here if you need

            var mapper = configuration.CreateMapper();

            foreach (PersonDto dto in dtoList)
            {
                peopleList.Add(mapper.Map<PersonEntity>(dto));
            }


            Console.WriteLine($"Name der 1. Person: {peopleList.FirstOrDefault()?.Name}");

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        Console.ReadLine();
    }
}