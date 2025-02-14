using Microsoft.AspNetCore.Http.Internal;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests;
public class UserDataImportHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISiteService> _siteServiceMock = new();

    private readonly UserDataImportHandler _sut;
    private const string UsersHeader = "User,Site";

    public UserDataImportHandlerTests()
    {
        _sut = new UserDataImportHandler(_userServiceMock.Object, _siteServiceMock.Object);
    }

    [Fact]
    public async Task CanReadUserData()
    {
        string[] inputRows =
        [
            "test1@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test1@nhs.net,308d515c-2002-450e-b248-4ba36f6667bb",
            "test2@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19",
            "test2@nhs.net,9a06bacd-e916-4c10-8263-21451ca751b8",
        ];

        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var sites = new List<Site>();

        foreach (var row in inputRows)
        {
            sites.Add(new Site(row.Split(',')[1], "Test", "Test Address", "07777777777", "ABC123", "Test Region", "ICB", "", [], new Location("Test", [1.0, 60.0])));
        }

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1])
            .ReturnsAsync(sites[2])
            .ReturnsAsync(sites[3]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(4);
        report.All(r => r.Success).Should().BeTrue();
    }

    // TODO: Add test around incorrect siteIds
}
