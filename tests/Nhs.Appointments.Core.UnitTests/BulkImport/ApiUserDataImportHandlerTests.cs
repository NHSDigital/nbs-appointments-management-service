using Microsoft.AspNetCore.Http;
using System.Text;

namespace Nhs.Appointments.Core.UnitTests.BulkImport;
public class ApiUserDataImportHandlerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();

    private readonly ApiUserDataImportHandler _sut;
    private const string ApiUserHeader = "ClientId,ApiSigningKey";

    public ApiUserDataImportHandlerTests()
    {
        _sut = new ApiUserDataImportHandler(_userServiceMock.Object);
    }

    [Fact]
    public async Task CanReadApiUserData()
    {
        string[] inputRows =
        [
            "test1,ABC123",
            "test2,DEF345"
        ];

        var input = CsvFileBuilder.BuildInputCsv(ApiUserHeader, inputRows);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
        var file = new FormFile(stream, 0, stream.Length, "Test", "test.csv");

        var report = await _sut.ProcessFile(file);

        report.Count().Should().Be(2);
        report.All(r => r.Success).Should().BeTrue();
    }
}
