using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.Features;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests;
public class UserDataImportHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ISiteService> _siteServiceMock = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelperMock = new();
    private readonly Mock<IOktaService> _oktaServiceMock = new();
    private readonly Mock<IEmailWhitelistStore> _emailWhitelistStore = new();

    private readonly UserDataImportHandler _sut;
    private const string UsersHeader = "User,FirstName,LastName,Site,appointment-manager,availability-manager,site-details-manager,user-manager";

    public UserDataImportHandlerTests()
    {
        _sut = new UserDataImportHandler(
            _userServiceMock.Object,
            _siteServiceMock.Object,
            _featureToggleHelperMock.Object,
            _oktaServiceMock.Object,
            _emailWhitelistStore.Object
        );
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
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@nhs.net"]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(4);
        report.All(r => r.Success).Should().BeTrue();

        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test1@nhs.net", $"site:{sites[0].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(1));
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test1@nhs.net", $"site:{sites[1].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(1));
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test2@nhs.net", $"site:{sites[2].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(1));
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test2@nhs.net", $"site:{sites[3].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Exactly(1));
    }

    [Fact]
    public async Task OktaIsDisabled_OktaUsersNotProcessed()
    {
        string[] inputRows =
        [
            "test1@okta.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test2@okta.net,,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(false);

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.First().Success.Should().BeFalse();

        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test1@okta.net", $"site:{sites[0].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Never);
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test2@okta.net", $"site:{sites[1].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Never);
    }

    [Fact]
    public async Task OktaIsEnabled_OktaUsersProcessed()
    {
        string[] inputRows =
        [
            "test1@okta.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test2@okta.net,Jane,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _oktaServiceMock.Setup(x => x.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UserProvisioningStatus { Success = true });
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@okta.net"]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.All(x => x.Success).Should().BeTrue();

        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test1@okta.net", $"site:{sites[0].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        _userServiceMock.Verify(u => u.UpdateUserRoleAssignmentsAsync("test2@okta.net", $"site:{sites[1].Id}", It.IsAny<IEnumerable<RoleAssignment>>()), Times.Once);
        _emailWhitelistStore.Verify(e => e.GetWhitelistedEmails(), Times.Once);
    }

    [Fact]
    public async Task CanValidateOktaUser()
    {
        string[] inputRows =
        [
            "test1@okta.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test1@okta.net,,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
            "test2@okta.net,Jane,,d3793464-b421-41f3-9bfa-53b06e7b3d19,,false,true,true,true",
            "test2@okta.net,,,9a06bacd-e916-4c10-8263-21451ca751b8,false,true,true,true",
        ];
    
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");
        var expectedErrors = 3;

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1])
            .ReturnsAsync(sites[2])
            .ReturnsAsync(sites[3]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@okta.net"]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(expectedErrors);
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
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@nhs.net"]);

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
        _userServiceMock.SetupSequence(u => u.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(false, string.Empty, ["test-role:one", "test-role:two"]));
        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@nhs.net"]);

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
            "test1@nhs.net,,,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, test, true, true",
            "test1@nhs.net,,,308d515c-2002-450e-b248-4ba36f6667bb, true, false, test, true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");
        var sites = GetSites(); 
        
        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.All(r => r.Success).Should().BeFalse();
        report.First().Message.Should().Contain("Invalid bool string format: test");
    }

    [Fact]
    public async Task DataReportsMissingColumns()
    {
        const string invalidHeaders = "Site,appointment-manager,availability-manager,site-details-manager";

        string[] inputRows =
        [
            "d3793464-b421-41f3-9bfa-53b06e7b3d19, false, true, true",
            "308d515c-2002-450e-b248-4ba36f6667bb, true, false, true"
        ];

        var input = CsvFileBuilder.BuildInputCsv(invalidHeaders, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(1);
        report.First().Message.Should().Contain("Error trying to parse CSV file: Header with name 'User'[0] was not found");
    }

    [Fact]
    public async Task ReadsUserData_AndAddsUserToOkta()
    {
        string[] inputRows =
        [
            "test1@okta.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test2@okta.net,Jane,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
            "test3@okta-with-trailing-white-space.net ,Jane,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _oktaServiceMock.Setup(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UserProvisioningStatus { Success = true });
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@okta.net", "@okta-with-trailing-white-space.net "]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(3);

        _oktaServiceMock.Verify(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        _emailWhitelistStore.Verify(e => e.GetWhitelistedEmails(), Times.Once);
        // Verify email whitespace is trimmed before calling Okta
        _oktaServiceMock.Verify(s => s.CreateIfNotExists("test3@okta-with-trailing-white-space.net", "Jane", "Smith"), Times.Once);
    }

    [Fact]
    public async Task ReadsUserData_AndReportsUnsuccessfulOktaReason_AndDoesntCallUserService()
    {
        string[] inputRows =
        [
            "test1@okta.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test2@okta.net,Jane,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _oktaServiceMock.Setup(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UserProvisioningStatus { Success = false, FailureReason = "Test failure reason." });
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@okta.net"]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(4);
        report.All(r => r.Success).Should().BeFalse();
        report.First(r => !r.Success).Message.Should().Be("Failed to create or update OKTA user. Failure reason: Test failure reason.");

        _oktaServiceMock.Verify(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        _userServiceMock.Verify(s => s.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()), Times.Never);
        _emailWhitelistStore.Verify(e => e.GetWhitelistedEmails(), Times.Once);
    }

    [Fact]
    public async Task ReadsUserData_AndReportsUnsuccesfulOktaCreation_WhenEmailDomainNotWhitelisted()
    {
        string[] inputRows =
        [
            "test1@invalid-domain.net,Jane,Smith,d3793464-b421-41f3-9bfa-53b06e7b3d19,false,true,true,true",
            "test2@another-invalid-domain.com,Jane,Smith,308d515c-2002-450e-b248-4ba36f6667bb,true,false,false,true",
        ];
        var input = CsvFileBuilder.BuildInputCsv(UsersHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _featureToggleHelperMock.Setup(x => x.IsFeatureEnabled(It.IsAny<string>())).ReturnsAsync(true);

        var sites = GetSites();

        _siteServiceMock.SetupSequence(s => s.GetSiteByIdAsync(It.IsAny<string>(), "*"))
            .ReturnsAsync(sites[0])
            .ReturnsAsync(sites[1]);
        _userServiceMock.Setup(x => x.UpdateUserRoleAssignmentsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<RoleAssignment>>()))
            .ReturnsAsync(new UpdateUserRoleAssignmentsResult(true, string.Empty, Array.Empty<string>()));
        _oktaServiceMock.Setup(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new UserProvisioningStatus { Success = false, FailureReason = "Test failure reason." });
        _emailWhitelistStore.Setup(x => x.GetWhitelistedEmails())
            .ReturnsAsync(["@okta.net"]);

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.All(r => r.Success).Should().BeFalse();
        report.First(r => !r.Success).Message.Should().Be("The following email domain: test1@invalid-domain.net is not included in the email domain whitelist.");

        _oktaServiceMock.Verify(s => s.CreateIfNotExists(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private List<Site> GetSites()
    {
        var sites = new List<Site>();

        foreach (var row in InputRows)
        {
            sites.Add(new Site(row.Split(',')[3], "Test", "Test Address", "07777777777", "ABC123", "Test Region", "ICB", "", [], new Location("Test", [1.0, 60.0])));
        }

        return sites;
    }

    private readonly string[] InputRows =
    [
        "test1@nhs.net,,,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, true, true, true",
        "test1@nhs.net,,,308d515c-2002-450e-b248-4ba36f6667bb, true, false, false, true",
        "test2@nhs.net,,,d3793464-b421-41f3-9bfa-53b06e7b3d19, false, true, true, true",
        "test2@nhs.net,,,9a06bacd-e916-4c10-8263-21451ca751b8, false, true, true, true",
    ];
}
