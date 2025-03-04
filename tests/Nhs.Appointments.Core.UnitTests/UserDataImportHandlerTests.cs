using Microsoft.AspNetCore.Http.Internal;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests;
public class UserDataImportHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISiteService> _siteServiceMock = new();

    private readonly UserDataImportHandler _sut;
    private const string UsersHeader = "User,Site,appointment-manager,availability-manager,site-details-manager,user-manager";

    public UserDataImportHandlerTests()
    {
        _sut = new UserDataImportHandler(_userServiceMock.Object, _siteServiceMock.Object);
    }

    [Fact]
    public async Task CanReadUserData()
    {
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, InputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1])
            .ReturnsAsync(sites[2])
            .ReturnsAsync(sites[3]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), "site", It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(4);
        report.All(r => r.Success).Should().BeTrue();

        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test1@nhs.net", "site", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(2));
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test2@nhs.net", "site", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReportsIncorrectSiteId_WhenNotFound()
    {
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, InputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var sites = GetSites();

        _siteServiceMock.Setup(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(null as Site);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(3);
        report.All(r => r.Success).Should().BeFalse();
        report.First().Message.Should().Be("The following site ID doesn't currently exist in the system: d3793464-b421-41f3-9bfa-53b06e7b3d19.");
    }

    [Fact]
    public async Task ReportsInvalidUserRoles()
    {
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, InputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1])
            .ReturnsAsync(sites[2])
            .ReturnsAsync(sites[3]);
        _userServiceMock.SetupSequence(u => u.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), "site", It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(false, string.Empty, ["test-role:one", "test-role:two"]));

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(5);
        report.All(r => r.Success).Should().BeFalse();
        report.Last().Success.Should().BeFalse();
        report.Last().Message.Should().Be("Failed to update user roles. The following roles are not valid: test-role:one|test-role:two");
    }

    [Fact]
    public async Task ReportsInvalidCsvFile()
    {
        string[] inputRows =
        [
            "test1@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, test, true, true",
            "test1@nhs.net,308d515c-2002-450e-b248-4ba36f6667bb, true, false, test, true",
        ];

        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var sites = GetSites(); var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.All(r => r.Success).Should().BeFalse();
        report.First().Message.Should().Contain("CsvHelper.ReaderException: An unexpected error occurred.").And.Contain("Invalid bool string format:  test");
    }

    private List<Site> GetSites()
    {
        var sites = new List<Site>();

        foreach (var row in InputRows)
        {
            sites.Add(new Site(row.Split(',')[1], "Test", "Test Address", "07777777777", "ABC123", "Test Region", "ICB", "", [], new Location("Test", [1.0, 60.0])));
        }

        return sites;
    }

    private readonly string[] InputRows =
        [
            "test1@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, true, true, true",
            "test1@nhs.net,308d515c-2002-450e-b248-4ba36f6667bb, true, false, false, true",
            "test2@nhs.net,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, true, true, true",
            "test2@nhs.net,9a06bacd-e916-4c10-8263-21451ca751b8, false, true, true, true",
        ];
}
