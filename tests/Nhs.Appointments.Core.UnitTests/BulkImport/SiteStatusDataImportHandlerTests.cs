using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.BulkImport;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests.BulkImport;
public class SiteStatusDataImportHandlerTests
{
    private readonly Mock<ISiteService> _siteServiceMock = new();

    private readonly SiteStatusDataImportHandler _sut;

    private const string Headers = "Id,Name";

    public SiteStatusDataImportHandlerTests()
    {
        _sut = new SiteStatusDataImportHandler(_siteServiceMock.Object);
    }

    [Fact]
    public async Task CanReadSiteData()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        string[] inputRows =
        [
            $"{id1},Test Site 1",
            $"{id2},Test Site 2",
        ];

        var sites = new List<Site>
        {
            new(
                id1.ToString(),
                "Test Site 1",
                "Addres 1",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty),
            new(
                id2.ToString(),
                "Test Site 2",
                "Addres 2",
                "01234567890",
                "OD21",
                "R2",
                "ICB2",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty)
        };

        var input = CsvFileBuilder.BuildInputCsv(Headers, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _siteServiceMock.Setup(x => x.ToggleSiteSoftDeletionAsync(It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(true));
        _siteServiceMock.Setup(x => x.GetAllSites(true, It.IsAny<bool>()))
            .ReturnsAsync(sites);

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(2);
        result.All(r => r.Success).Should().BeTrue();
    }

    [Fact]  
    public async Task InvalidSiteId_ReportsBadData()
    {
        string[] inputRows =
        [
            "invalidSiteId,Test Site 1",
            "anotherInvalidSiteId,Test Site 2"
        ];

        var input = CsvFileBuilder.BuildInputCsv(Headers, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(2);
        result.All(r => r.Success).Should().BeFalse();
        result.First().Message.Should().Be("Invalid GUID string format for Site field: 'invalidSiteId'");
    }

    [Fact]
    public async Task InvalidSiteName_ReportsBadData()
    {
        string[] inputRows =
        [
            $"{Guid.NewGuid()},",
            $"{Guid.NewGuid()},"
        ];

        var input = CsvFileBuilder.BuildInputCsv(Headers, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(2);
        result.All(r => r.Success).Should().BeFalse();
        result.First().Message.Should().Be("Site name must have a value.");
    }

    [Fact]
    public async Task FailsToMatchSite_ReportsBadData()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        string[] inputRows =
        [
            $"{id1},Test Site 1",
            $"{id2},Test Site 2",
        ];

        var sites = new List<Site>
        {
            new(
                Guid.NewGuid().ToString(),
                "Test Site 1",
                "Addres 1",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty),
            new(
                Guid.NewGuid().ToString(),
                "Test Site 2",
                "Addres 2",
                "01234567890",
                "OD21",
                "R2",
                "ICB2",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty)
        };

        var input = CsvFileBuilder.BuildInputCsv(Headers, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _siteServiceMock.Setup(x => x.ToggleSiteSoftDeletionAsync(It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(true));
        _siteServiceMock.Setup(x => x.GetAllSites(true, It.IsAny<bool>()))
            .ReturnsAsync(sites);

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(2);
        result.All(r => r.Success).Should().BeFalse();
        result.First().Message.Should().Be($"Could not find existing site with name Test Site 1 and ID: {id1}");
    }

    [Fact]
    public async Task FailsToUpdateSiteSoftDeletionStatus_ReportsFailure()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        string[] inputRows =
        [
            $"{id1},Test Site 1",
            $"{id2},Test Site 2",
        ];

        var sites = new List<Site>
        {
            new(
                id1.ToString(),
                "Test Site 1",
                "Addres 1",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty),
            new(
                id2.ToString(),
                "Test Site 2",
                "Addres 2",
                "01234567890",
                "OD21",
                "R2",
                "ICB2",
                string.Empty,
                [],
                new Location("Test", [1.123, 3.321]),
                null,
                null,
                string.Empty)
        };

        var input = CsvFileBuilder.BuildInputCsv(Headers, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _siteServiceMock.Setup(x => x.ToggleSiteSoftDeletionAsync(It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(false));
        _siteServiceMock.Setup(x => x.GetAllSites(true, It.IsAny<bool>()))
            .ReturnsAsync(sites);

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(4);
        result.All(r => r.Success).Should().BeFalse();
    }
}
