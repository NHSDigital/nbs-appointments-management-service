using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.BulkImport;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests.BulkImport;
public class AdminUserDataImportHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();

    private readonly AdminUserDataImportHandler _sut;
    private const string AdminUserHeader = "Email";

    public AdminUserDataImportHandlerTests()
    {
        _sut = new AdminUserDataImportHandler(_userServiceMock.Object);
    }

    [Fact]
    public async Task ReportsEmailAddressNotProvided()
    {
        string[] inputRows = [ "test.user@nhs.net", " " ];
        var input = CsvFileBuilder.BuildInputCsv(AdminUserHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(1);
        result.All(r => r.Success).Should().BeFalse();
        result.Last().Message.Should().Be("Value cannot be null. (Parameter 'Email field must have a value.')");
    }

    [Fact]
    public async Task ReportsInvalidEmailAddress()
    {
        string[] inputRows = ["invalid-email-address"];
        var input = CsvFileBuilder.BuildInputCsv(AdminUserHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(1);
        result.All(r => r.Success).Should().BeFalse();
        result.First().Message.Should().Be("Admin user email: 'invalid-email-address' is not a valid email address.");
    }

    [Fact]
    public async Task ReportsInvalidEmailDomain()
    {
        string[] inputRows = ["invalid.email@domain.com"];
        var input = CsvFileBuilder.BuildInputCsv(AdminUserHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(1);
        result.All(r => r.Success).Should().BeFalse();
        result.First().Message.Should().Be("Email must be an nhs.net email domain. Current email: 'invalid.email@domain.com'");
    }

    [Fact]
    public async Task SavesAdminUser_WhenTheyDontAlreadyExist_AndConvertsEmailToLowercase()
    {
        string[] inputRows =
        [
            "USER.doesnt.exIST1@nhs.net",
            "User.DoESnT.Exist2@NHS.net"
        ];
        var input = CsvFileBuilder.BuildInputCsv(AdminUserHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _userServiceMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(null as User);

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(2);
        result.All(r => r.Success).Should().BeTrue();

        _userServiceMock.Verify(x => x.SaveAdminUserAsync("user.doesnt.exist1@nhs.net"), Times.Once);
        _userServiceMock.Verify(x => x.SaveAdminUserAsync("user.doesnt.exist2@nhs.net"), Times.Once);
    }

    [Fact]
    public async Task RemovesAdminUser_WhenTheyAlreadyExist()
    {
        string[] inputRows = ["user.already.exists@nhs.net"];
        var input = CsvFileBuilder.BuildInputCsv(AdminUserHeader, inputRows);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        _userServiceMock.Setup(x => x.GetUserAsync(It.IsAny<string>()))
            .ReturnsAsync(new User
            {
                Id = "user.already.exists@nhs.net",
                RoleAssignments =
                [
                    new()
                    {
                        Role = "system:admin-user",
                        Scope = "global"
                    }
                ]
            });

        var result = await _sut.ProcessFile(file);

        result.Count().Should().Be(1);
        result.All(r => r.Success).Should().BeTrue();

        _userServiceMock.Verify(x => x.RemoveAdminUserAsync("user.already.exists@nhs.net"), Times.Once);
    }
}
