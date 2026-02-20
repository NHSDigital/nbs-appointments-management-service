using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Nhs.Appointments.Audit.Persistance;
using Nhs.Appointments.Audit.Services;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance;

namespace Nhs.Appointments.Api.Tests.Audit;

public class ServiceTests
{
    private readonly Mock<ITypedDocumentCosmosStore<AuditFunctionDocument>> _auditFunctionDocumentStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditAuthDocument>> _auditAuthDocumentStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditNotificationDocument>> _auditNotificationDocumentStore = new();
    private readonly Mock<ITypedDocumentCosmosStore<AuditUserRemovedDocument>> _auditUserRemovedDocumentStore = new();

    private readonly Mock<CosmosClient> _mockCosmosClient = new();
    private readonly Mock<IMapper> _mockMapper = new();
    private readonly Mock<IMetricsRecorder> _mockMetrics = new();
    private readonly Mock<IOptions<CosmosDataStoreOptions>> _mockOptions = new();
    private readonly Mock<IOptions<ContainerRetryOptions>> _mockRetryOptions = new();
    private readonly Mock<ILastUpdatedByResolver> _mockLastUpdatedByResolver = new();
    private readonly Mock<ILogger<AuditFunctionDocument>> _funcLogger = new();
    private readonly Mock<ILogger<AuditAuthDocument>> _authLogger = new();
    private readonly Mock<ILogger<AuditNotificationDocument>> _notificationLogger = new();

    [Fact]
    public async Task RecordFunction_WriteAsync_IsCalled()
    {
        var sut = new AuditWriteService(
            _auditFunctionDocumentStore.Object, 
            _auditAuthDocumentStore.Object,
            _auditNotificationDocumentStore.Object,
            _auditUserRemovedDocumentStore.Object
        );

        var id = $"{Guid.NewGuid()}_{Guid.NewGuid()}";
        var user = "abd-123@test.com";
        var site = "site-A";
        var function = "AzureFunction";
        var timestamp = DateTime.UtcNow;

        await sut.RecordFunction(id, timestamp, user, function, site);

        _auditFunctionDocumentStore.Verify(x => x.WriteAsync(It.Is<AuditFunctionDocument>(y =>
            y.Id == id
            && y.FunctionName == function
            && y.Site == site
            && y.Timestamp == timestamp
            && y.User == user
        )), Times.Once);
    }

    [Fact]
    public async Task RecordAuth_LoginAction_WriteAsync_IsCalled()
    {
        var sut = new AuditWriteService(
            _auditFunctionDocumentStore.Object, 
            _auditAuthDocumentStore.Object,
            _auditNotificationDocumentStore.Object,
            _auditUserRemovedDocumentStore.Object
        );

        var id = $"{Guid.NewGuid()}_{Guid.NewGuid()}";
        var user = "abd-123@test.com";
        var timestamp = DateTime.UtcNow;

        await sut.RecordAuth(id, timestamp, user, AuditAuthActionType.Login);

        _auditAuthDocumentStore.Verify(x => x.WriteAsync(It.Is<AuditAuthDocument>(y =>
            y.Id == id
            && y.ActionType == AuditAuthActionType.Login
            && y.Timestamp == timestamp
            && y.User == user
        )), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("someReference")]
    public async Task RecordNotification_WriteAsync_IsCalled(string reference)
    {
        var sut = new AuditWriteService(
            _auditFunctionDocumentStore.Object, 
            _auditAuthDocumentStore.Object,
            _auditNotificationDocumentStore.Object, 
            _auditUserRemovedDocumentStore.Object
        );

        var id = $"{Guid.NewGuid()}_{Guid.NewGuid()}";
        var user = "abd-123@test.com";
        var timestamp = DateTime.UtcNow;
        var destinationId = "SomeIdentifier";
        var notificationName = "ANotification";
        var template = $"{Guid.NewGuid()}";
        var notificationType = "SMSorEmail";

        await sut.RecordNotification(id, timestamp, user, destinationId, notificationName, template, notificationType,
            reference);

        _auditNotificationDocumentStore.Verify(x => x.WriteAsync(It.Is<AuditNotificationDocument>(y =>
            y.Id == id
            && y.Timestamp == timestamp
            && y.User == user
            && y.DestinationId == destinationId
            && y.NotificationName == notificationName
            && y.Template == template
            && y.NotificationType == notificationType
            && y.Reference == reference
        )), Times.Once);
    }

    [Fact]
    public async Task RecordAuth_UndefinedAction_ThrowsArgumentException()
    {
        var sut = new AuditWriteService(
            _auditFunctionDocumentStore.Object, 
            _auditAuthDocumentStore.Object,
            _auditNotificationDocumentStore.Object,
            _auditUserRemovedDocumentStore.Object
        );

        var id = $"{Guid.NewGuid()}_{Guid.NewGuid()}";
        var user = "abd-123@test.com";
        var timestamp = DateTime.UtcNow;

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await sut.RecordAuth(id, timestamp, user, AuditAuthActionType.Undefined));
    }

    [Fact]
    public void AuditDocStore_SetsCosmosData_Correctly()
    {
        _mockOptions.Setup(x => x.Value).Returns(new CosmosDataStoreOptions { DatabaseName = "appts" });

        var auditDocStore = new TypedDocumentCosmosStore<AuditFunctionDocument>(
            _mockCosmosClient.Object,
            _mockOptions.Object,
            _mockRetryOptions.Object,
            _mockMapper.Object,
            _mockMetrics.Object,
            _mockLastUpdatedByResolver.Object,
            _funcLogger.Object);

        auditDocStore.DatabaseName.Should().Be("appts");
        auditDocStore.ContainerName.Should().Be("audit_data");
        auditDocStore.GetDocumentType().Should().Be("function");
    }

    [Fact]
    public void AuditNotificationStore_SetsCosmosData_Correctly()
    {
        _mockOptions.Setup(x => x.Value).Returns(new CosmosDataStoreOptions { DatabaseName = "appts" });

        var auditDocStore = new TypedDocumentCosmosStore<AuditNotificationDocument>(
            _mockCosmosClient.Object,
            _mockOptions.Object,
            _mockRetryOptions.Object,
            _mockMapper.Object,
            _mockMetrics.Object,
            _mockLastUpdatedByResolver.Object,
            _notificationLogger.Object);

        auditDocStore.DatabaseName.Should().Be("appts");
        auditDocStore.ContainerName.Should().Be("audit_data");
        auditDocStore.GetDocumentType().Should().Be("notification");
    }

    [Fact]
    public void AuditAuthStore_SetsCosmosData_Correctly()
    {
        _mockOptions.Setup(x => x.Value).Returns(new CosmosDataStoreOptions { DatabaseName = "appts" });

        var auditDocStore = new TypedDocumentCosmosStore<AuditAuthDocument>(
            _mockCosmosClient.Object,
            _mockOptions.Object,
            _mockRetryOptions.Object,
            _mockMapper.Object,
            _mockMetrics.Object,
            _mockLastUpdatedByResolver.Object,
            _authLogger.Object);

        auditDocStore.DatabaseName.Should().Be("appts");
        auditDocStore.ContainerName.Should().Be("audit_data");
        auditDocStore.GetDocumentType().Should().Be("auth");
    }
}
