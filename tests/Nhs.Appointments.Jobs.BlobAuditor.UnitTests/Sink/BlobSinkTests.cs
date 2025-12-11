using Moq;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Blob;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;

namespace Nhs.Appointments.Jobs.BlobAuditor.UnitTests.Sink;

public class BlobSinkTests
{
    private readonly Mock<IAzureBlobStorage> _mockAzureBlobStorage;
    private readonly Mock<TimeProvider> _mockTimeProvider;
    private readonly BlobSink _blobSink;

    public BlobSinkTests()
    {
        _mockAzureBlobStorage = new Mock<IAzureBlobStorage>();
        _mockTimeProvider = new Mock<TimeProvider>();
        _blobSink = new BlobSink(_mockAzureBlobStorage.Object, _mockTimeProvider.Object);
    }

    [Fact]
    public async Task Consume_ShouldCallAzureBlobStorageWithCorrectNames()
    {
        // Arrange
        var source = "audit_data";
        var now = new DateTimeOffset(2025, 12, 11, 10, 30, 0, TimeSpan.Zero);
        _mockTimeProvider.Setup(tp => tp.GetUtcNow()).Returns(now);

        var item = new JObject
        {
            ["docType"] = "patient",
            ["id"] = "123"
        };

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(memStream);

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        var expectedContainerName = $"{now:yyyyMMdd}-auditdata"; 
        _mockAzureBlobStorage.Verify(a => a.GetBlobUploadStream(
            expectedContainerName,
            It.Is<string>(s => s.Contains("patient") && s.Contains("123") && s.EndsWith(".json"))
        ), Times.Once);
    }

    [Fact]
    public async Task Consume_ShouldUseTimestampFromItem_When_tsExists()
    {
        // Arrange
        var source = "audit_data";
        var now = new DateTimeOffset(2025, 12, 11, 10, 30, 0, TimeSpan.Zero);
        _mockTimeProvider.Setup(tp => tp.GetUtcNow()).Returns(now);

        var tsValue = new DateTimeOffset(2023, 1, 2, 15, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var item = new JObject
        {
            ["_ts"] = tsValue,
            ["docType"] = "patient",
            ["id"] = "456"
        };

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(memStream);

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        var expectedContainerName = $"{now:yyyyMMdd}-auditdata";
        _mockAzureBlobStorage.Verify(a => a.GetBlobUploadStream(
            expectedContainerName,
            It.Is<string>(s => s.Contains("patient") && s.Contains("456"))
        ), Times.Once);
    }
}
