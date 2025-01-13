using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Api.Tests.Audit;

public class ServiceTests
{
    private readonly Mock<ITypedDocumentCosmosStore<AuditFunctionDocument>> _auditFunctionDocumentStore = new();

    private readonly Mock<CosmosClient> _mockCosmosClient = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMetricsRecorder> _mockMetrics = new();
    private readonly Mock<IOptions<CosmosDataStoreOptions>> _mockOptions = new();

    [Fact]
    public async Task RecordFunction_WriteAsync_IsCalled()
    {
        var sut = new AuditWriteService(_auditFunctionDocumentStore.Object);

        var id = $"{Guid.NewGuid()}_{Guid.NewGuid()}";
        var user = "abd-123@test.com";
        var site = "site-A";
        var function = "AzureFunction";
        var timestamp = DateTime.UtcNow;

        await sut.RecordFunction(id, timestamp, user, function, site);

        _auditFunctionDocumentStore.Verify(x => x.WriteAsync(It.Is<AuditFunctionDocument>(
            y =>
                y.Id == id
                && y.FunctionName == function
                && y.Site == site
                && y.Timestamp == timestamp
                && y.User == user
        )), Times.Once);
    }

    [Fact]
    public void AuditDocStore_SetsCosmosData_Correctly()
    {
        _mockOptions.Setup(x => x.Value).Returns(new CosmosDataStoreOptions { DatabaseName = "appts" });

        var auditDocStore = new TypedDocumentCosmosStore<AuditFunctionDocument>(_mockCosmosClient.Object,
            _mockOptions.Object, _mockMapper.Object, _mockMetrics.Object);

        auditDocStore._databaseName.Should().Be("appts");
        auditDocStore._containerName.Should().Be("audit_data");
        auditDocStore.GetDocumentType().Should().Be("function");
    }
}
