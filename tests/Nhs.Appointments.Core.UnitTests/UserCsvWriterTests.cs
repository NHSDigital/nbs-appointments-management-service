using Nhs.Appointments.Core.Reports.Users;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests;

public class UserCsvWriterTests
{
    private readonly Mock<TimeProvider> _timeProvider = new();

    private readonly UserCsvWriter _sut;

    public UserCsvWriterTests()
    {
        _timeProvider.Setup(x => x.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2026, 1, 1, 1, 1, 1)));
        _sut = new UserCsvWriter(_timeProvider.Object);
    }

    [Fact]
    public async Task CompilesEmptyFile_WhenNoUsersAssignedToSite()
    {
        var (FileName, FileContent) = await _sut.CompileSiteUsersReportCsv("site-123", Array.Empty<User>());

        FileName.Should().Be("UserReport_Site_site-123_20260101010101.csv");

        var fileContents = Encoding.UTF8.GetString(FileContent.ToArray());
        var csvLines = fileContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var headers = csvLines[0].Split(',');

        headers.Length.Should().Be(1);
        headers.First().Should().Be("User");

        csvLines.Length.Should().Be(1);
    }

    [Fact]
    public async Task CompilesFileWithUsers_WhenUsersAssignedToSite()
    {
        var (FileName, FileContent) = await _sut.CompileSiteUsersReportCsv("site-321", new[]
        {
            new User { Id = "test-user1@nhs.net" },
            new User { Id = "test-user2@nhs.net" }
        });

        FileName.Should().Be("UserReport_Site_site-321_20260101010101.csv");

        var fileContents = Encoding.UTF8.GetString(FileContent.ToArray());
        var csvLines = fileContents.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var headers = csvLines[0].Split(',');

        headers.Length.Should().Be(1);
        headers.First().Should().Be("User");

        csvLines.Length.Should().Be(3);
        csvLines[1].Should().Be("test-user1@nhs.net");
        csvLines[2].Should().Be("test-user2@nhs.net");
    }
}
