using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Blob;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;

namespace Nhs.Appointments.Jobs.BlobAuditor.UnitTests.Sink;

public class BlobSinkTests
{
    private readonly Mock<IAzureBlobStorage> _mockAzureBlobStorage = new();
    private readonly Mock<IItemExclusionProcessor> _itemExclusionProcessor = new();
    private readonly BlobSink _blobSink;
    private readonly Mock<ILogger<BlobSink>> _logger = new();

    public BlobSinkTests()
    {
        _blobSink = new BlobSink(
            _mockAzureBlobStorage.Object,
            _itemExclusionProcessor.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Consume_ShouldUseLastUpdatedOnTimestampFromItem_When_LastUpdatedOnExists()
    {
        // Arrange
        var source = "audit_data";
        var cosmosTsValue = new DateTimeOffset(2023, 1, 2, 15, 4, 2, TimeSpan.Zero).ToUnixTimeSeconds();

        var item = new JObject
        {
            ["_ts"] = cosmosTsValue,
            ["docType"] = "patient",
            ["id"] = "456",
            ["lastUpdatedOn"] = "2024-03-10T09:30:11.5872696Z",
        };

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(memStream);

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        _mockAzureBlobStorage.Verify(a => a.GetBlobUploadStream(
            "20240310-auditdata",
            "patient/0930115872696-456.json"
        ), Times.Once);
    }
    
    [Fact]
    public async Task Consume_ShouldRecord_When_LastUpdatedNotExists_ButTsDoes()
    {
        // Arrange
        var source = "audit_data";

        var tsValue = new DateTimeOffset(2023, 1, 2, 15, 4, 2, TimeSpan.Zero).ToUnixTimeSeconds();
        var item = new JObject
        {
            ["_ts"] = tsValue,
            ["docType"] = "patient",
            ["id"] = "789"
        };

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(memStream);

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        _mockAzureBlobStorage.Verify(a => a.GetBlobUploadStream(
            "20230102-auditdata",
            "patient/1504020000000-789.json"
        ), Times.Once);
    }
    
    [Fact]
    public async Task Consume_ShouldNotRecord_When_NeitherLastUpdatedNorTs_Exists()
    {
        // Arrange
        var source = "audit_data";

        var item = new JObject
        {
            ["id"] = "456",
            ["docType"] = "patient",
        };

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(memStream);

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        _mockAzureBlobStorage.Verify(a => a.GetBlobUploadStream(
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
        
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Auditor cannot process document with id: '456' and docType: 'patient', as both lastUpdatedOn and _ts are null")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }

    [Fact]
    public async Task Consume_CallsAzureBlobStorage_WithExclusionProcessedItem()
    {
        // Arrange
        var source = "audit_data";
        var item = new JObject
        {
            ["docType"] = "patient",
            ["id"] = "123",
            ["contactDetails"] = "some private information",
            ["lastUpdatedOn"] = "2026-03-10T09:30:11.5872696Z",
        };
        var exclusionProcessedItem = new JObject
        {
            ["docType"] = "patient",
            ["id"] = "123",
            ["lastUpdatedOn"] = "2026-03-10T09:30:11.5872696Z",
        };
        string capturedContent = null;
        _itemExclusionProcessor.Setup(x => x.Apply(It.IsAny<string>(), It.IsAny<JObject>())).Returns(exclusionProcessedItem);

        var memStream = new MemoryStream();
        _mockAzureBlobStorage
            .Setup(a => a.GetBlobUploadStream("20260310-auditdata", "patient/0930115872696-123.json"))
            .ReturnsAsync(memStream)
            .Callback<string, string>(async (_, __) =>
            {
                using var tempStream = new MemoryStream();
                await tempStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(exclusionProcessedItem, Formatting.Indented)));
                tempStream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(tempStream);
                capturedContent = await reader.ReadToEndAsync();
            });

        // Act
        await _blobSink.Consume(source, item);

        // Assert
        var expectedContent = JsonConvert.SerializeObject(exclusionProcessedItem, Formatting.Indented);
        Assert.Equal(expectedContent, capturedContent);
    }
}
