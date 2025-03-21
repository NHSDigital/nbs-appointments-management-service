using Microsoft.AspNetCore.Http.Internal;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests;
public class SiteDataImporterHandlerTests
{
    private readonly Mock<ISiteService> _siteServiceMock = new();
    private readonly Mock<IWellKnowOdsCodesService> _wellKnownOdsCodesServiceMock = new();

    private readonly SiteDataImporterHandler _sut;

    private const string SitesHeader =
        "Id,OdsCode,Name,Address,PhoneNumber,Longitude,Latitude,ICB,Region,Site type,accessible_toilet,braille_translation_service,disabled_car_parking,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access";

    public SiteDataImporterHandlerTests()
    {
        _sut = new SiteDataImporterHandler(_siteServiceMock.Object, _wellKnownOdsCodesServiceMock.Object);
    }

    [Fact]
    public async Task CanReadSiteData()
    {
        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, ValidInputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("site3", "Site 3", "Test3"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(3);
        report.All(r => r.Success).Should().BeTrue();
    }

    [Fact]
    public async Task InvalidSiteID_DataReportsBadData()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        string[] inputRows =
        [
            "ferfgsd,site1,\"test site 1\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb1\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            "sadfsdafsdf,site2,\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb2\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false"
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.First().Message.Should().StartWith($"CsvHelper.TypeConversion.TypeConverterException: Invalid GUID string format: ferfgsd");
        report.Last().Message.Should().StartWith($"CsvHelper.TypeConversion.TypeConverterException: Invalid GUID string format: sadfsdafsdf");
        report.All(r => r.Success).Should().BeFalse();
    }

    [Fact]
    public async Task InvalidLongLat_DataReportsBadData()
    {
        var id1 = Guid.NewGuid().ToString();
        var id2 = Guid.NewGuid().ToString();

        string[] inputRows =
        [
            $"\"{id1}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"foo\",\"bar\",\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{id2}\",\"site2\",\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"True\",false,true,true,false"
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.All(r => r.Success ).Should().BeFalse();
        report.First().Message.Should().Contain("Text: 'foo'");
    }

    [Fact]
    public async Task DuplicateSitesIds_DataReportsBadData()
    {
        var id = Guid.NewGuid().ToString();

        string[] inputRows =
        [
            $"\"{id}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{id}\",\"site2\",\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().StartWith($"Duplicate site Id provided: {id}. SiteIds must be unique.");
        report.Count(r => !r.Success).Should().Be(1);
    }

    [Fact]
    public async Task DataReportsOdsCodesNotFound()
    {
        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, ValidInputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("site75", "Site 75", "Test75"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.Last().Message.Should().StartWith($"Provided site ODS code: site3 not found in the well known ODS code list.");
    }

    [Fact]
    public async Task DataReportsInvalidLongitudeCoordinates_WhenOutOfRange()
    {
        const double invalidLongitudeUpper = 2.32;
        const double invalidLongitudeLower = -9.24;

        var inputRows = new string[]
        {
            $"\"{Guid.NewGuid()}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"{invalidLongitudeUpper}\",\"54.12\",\"test icb1\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{Guid.NewGuid()}\",\"site2\",\"test site 2\",\"123 test street\",\"01234 567890\",{invalidLongitudeLower},52.43,\"test icb2\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        };

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.First().Message.Should().Contain($"Longitude: {invalidLongitudeUpper} is not a valid UK longitude.");
        report.Last().Message.Should().Contain($"Longitude: {invalidLongitudeLower} is not a valid UK longitude.");
    }

    [Fact]
    public async Task DataReportsInvalidLatitudeCoordinates_WhenOutOfRange()
    {
        const double invalidLatitudeUpper = 61.75;
        const double invalidLatitudeLower = 49.12;

        var inputRows = new string[]
        {
            $"\"{Guid.NewGuid()}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"0.50\",\"{invalidLatitudeUpper}\",\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{Guid.NewGuid()}\",\"site2\",\"test site 2\",\"123 test street\",\"01234 567890\",-1.75,{invalidLatitudeLower},\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        };

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.First().Message.Should().Contain($"Latitude: {invalidLatitudeUpper} is not a valid UK latitude.");
        report.Last().Message.Should().Contain($"Latitude: {invalidLatitudeLower} is not a valid UK latitude.");
    }

    [Fact]
    public async Task DataReportsMissingColumns()
    {
        const string invalidHeaders = "Id,Name,Address,Longitude,Latitude,ICB,Region,accessible_toilet,car_parking,induction_loop,sign_language_service,step_free_access,text_relay,wheelchair_access";

        string[] inputRows =
        [
            $"\"{Guid.NewGuid()}\",\"site1\",\"123 test street\",\"0.50\",\"60.0\",\"test icb\",\"Yorkshire\",true,True,False,false,true,false,true",
            $"\"{Guid.NewGuid()}\",\"site2\",\"321 test street\",\"0.75\",\"59.5\",\"test icb\",\"Yorkshire\",true,True,False,false,true,false,true"
        ];

        var input = CsvFileBuilder.BuildInputCsv(invalidHeaders, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().Contain("Error trying to parse CSV file: Header with name 'OdsCode'[0] was not found");
    }

    [Fact]
    public async Task DuplicateSiteNamesReportsBadData()
    {
        const string site = "Test Site 1";

        string[] inputRows =
        [
            $"\"{Guid.NewGuid()}\",\"site1\",\"{site}\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{Guid.NewGuid()}\",\"site2\",\"{site}\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().StartWith($"Duplicate site name provided: {site}. Site names must be unique.");
        report.Count(r => !r.Success).Should().Be(1);
    }

    [Fact]
    public async Task ReportsBadDataWhenICBCodeIsNotValid()
    {
        const string icb = "invalid icb";

        string[] inputRows =
        [
            $"\"{Guid.NewGuid()}\",\"site1\",\"Test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"{icb}\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{Guid.NewGuid()}\",\"site2\",\"Test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().StartWith($"Provided site ICB code: {icb} not found in the well known ICB code list.");
        report.Count(r => !r.Success).Should().Be(1);
    }

    [Fact]
    public async Task ReportBadDataWhenRegionIsNotValid()
    {
        const string region = "reg01";

        string[] inputRows =
        [
            $"\"{Guid.NewGuid()}\",\"site1\",\"Test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb\",\"{region}\",,true,True,False,false,\"true\",false,true,true,false",
            $"\"{Guid.NewGuid()}\",\"site2\",\"Test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("Yorkshire", "Site 4", "Region"),
                new("test icb", "Site 5", "ICB")
            });

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().StartWith($"Provided region: {region} not found in the well known Region list.");
        report.Count(r => !r.Success).Should().Be(1);
    }

    [Fact]
    public async Task ReportsSiteAlreadyExists_AndDoesntCallTheSiteService()
    {
        var siteId = Guid.NewGuid();

        string[] inputRows =
        [
            $"\"{siteId}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb1\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        ];

        var input = CsvFileBuilder.BuildInputCsv(SitesHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _wellKnownOdsCodesServiceMock.Setup(x => x.GetWellKnownOdsCodeEntries())
            .ReturnsAsync(new List<WellKnownOdsEntry>
            {
                new("site1", "Site 1", "Test1"),
                new("site2", "Site 2", "Test2"),
                new("site3", "Site 3", "Test3")
            });
        _siteServiceMock.Setup(x => x.GetSiteByIdAsync(siteId.ToString(), "*"))
            .ReturnsAsync(new Site(siteId.ToString(), "Site1", "123 test street", "01234 567890", "ODS", "Region", "test icb", "", [], new("Test", [60.0, 1.5])));

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().Be($"Site with ID: {siteId} already exists in the system.");
        report.All(r => r.Success).Should().BeFalse();

        _siteServiceMock.Verify(s => s.SaveSiteAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Location>(),
            It.IsAny<List<Accessibility>>()), Times.Never);
    }

    private readonly string[] ValidInputRows =
    [
        $"\"{Guid.NewGuid()}\",\"site1\",\"test site 1\",\"123 test street\",\"01234 567890\",\"1.0\",\"60.0\",\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        $"\"{Guid.NewGuid()}\",\"site2\",\"test site 2\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,True,False,false,\"true\",false,true,true,false",
        $"\"{Guid.NewGuid()}\",\"site3\",\"test site 3\",\"123 test street\",\"01234 567890\",1.0,60.0,\"test icb\",\"Yorkshire\",,true,true,False,\"false\",\"true\",false,true,true,false",
    ];
}
