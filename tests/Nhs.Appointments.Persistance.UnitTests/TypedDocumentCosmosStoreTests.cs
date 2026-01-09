using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Persistance.UnitTests;

public class TypedDocumentCosmosStoreTests
{
    private readonly Mock<CosmosClient> _cosmosClient = new();
    private readonly Mock<Container> _container = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ILastUpdatedByResolver> _lastUpdatedByResolver = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly IOptions<CosmosDataStoreOptions> _options = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db" });

    private readonly TypedDocumentCosmosStore<TestDocument> _sut;

    public TypedDocumentCosmosStoreTests()
    {
        _cosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_container.Object);

        _sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _featureToggleHelper.Object);
    }

    [Fact]
    public async Task WriteAsync_AuditLastUpdatedByFlagNotEnabled_LastUpdatedByNotUpdated()
    {
        // Arrange
        var doc = _sut.NewDocument();
        doc.LastUpdatedBy = null;

        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(Flags.AuditLastUpdatedBy))
            .ReturnsAsync(false);

        var mockResponse = new Mock<ItemResponse<TestDocument>>();
        mockResponse.Setup(r => r.RequestCharge).Returns(1.0);

        _container
            .Setup(x => x.UpsertItemAsync(
                It.IsAny<TestDocument>(), 
                It.IsAny<PartitionKey?>(), 
                It.IsAny<ItemRequestOptions>(), 
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockResponse.Object);

        // Act
        await _sut.WriteAsync(doc);

        // Assert
        doc.LastUpdatedBy.Should().BeNull();
        _lastUpdatedByResolver.Verify(x => x.GetLastUpdatedBy(), Times.Never);
    }

    [Fact]
    public async Task WriteAsync_AuditLastUpdatedByFlagEnabled_LastUpdatedByIsUpdated()
    {
        // Arrange
        var testUser = "user@test.com";
        var doc = _sut.NewDocument();

        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(Flags.AuditLastUpdatedBy))
            .ReturnsAsync(true);

        _lastUpdatedByResolver
            .Setup(x => x.GetLastUpdatedBy())
            .Returns(testUser);

        var mockResponse = new Mock<ItemResponse<TestDocument>>();
        mockResponse.Setup(r => r.RequestCharge).Returns(1.0);

        _container
            .Setup(x => x.UpsertItemAsync(
                It.IsAny<TestDocument>(), 
                It.IsAny<PartitionKey?>(), 
                It.IsAny<ItemRequestOptions>(), 
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockResponse.Object);

        // Act
        await _sut.WriteAsync(doc);

        // Assert
        doc.LastUpdatedBy.Should().Be(testUser);
        _container.Verify(x => x.UpsertItemAsync(
            doc, 
            It.IsAny<PartitionKey?>(), 
            It.IsAny<ItemRequestOptions>(), 
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task PatchDocument_AuditEnabled_IncludesLastUpdatedByOperation()
    {
        // Arrange
        var testUser = "patch-user@test.com";
        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(Flags.AuditLastUpdatedBy))
            .ReturnsAsync(true);

        _lastUpdatedByResolver
            .Setup(x => x.GetLastUpdatedBy())
            .Returns(testUser);

        IReadOnlyList<PatchOperation> capturedPatches = null;
        _container
            .Setup(x => x.PatchItemAsync<TestDocument>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<IReadOnlyList<PatchOperation>>(),
                It.IsAny<PatchItemRequestOptions>(),
                default))
            .Callback<string, PartitionKey, IReadOnlyList<PatchOperation>, PatchItemRequestOptions, CancellationToken>(
                (id, pk, patches, opts, ct) => capturedPatches = patches)
            .ReturnsAsync(Mock.Of<ItemResponse<TestDocument>>());

        // Act
        await _sut.PatchDocument("pk", "id", PatchOperation.Replace("/name", "New Name"));

        // Assert
        capturedPatches.Should().HaveCount(2);
        capturedPatches.Should().ContainSingle(p =>
            p.OperationType == PatchOperationType.Set &&
            p.Path == "/lastUpdatedBy");
    }

    [Fact]
    public async Task PatchDocument_AuditDisabled_DoesNotIncludeLastUpdatedByOperation()
    {
        // Arrange
        _featureToggleHelper
            .Setup(x => x.IsFeatureEnabled(Flags.AuditLastUpdatedBy))
            .ReturnsAsync(false);

        IReadOnlyList<PatchOperation> capturedPatches = null;
        _container
            .Setup(x => x.PatchItemAsync<TestDocument>(
                It.IsAny<string>(), 
                It.IsAny<PartitionKey>(), 
                It.IsAny<IReadOnlyList<PatchOperation>>(), 
                It.IsAny<PatchItemRequestOptions>(), 
                default
            ))
            .Callback<string, PartitionKey, IReadOnlyList<PatchOperation>, PatchItemRequestOptions, CancellationToken>((id, pk, patches, opts, ct) => capturedPatches = patches)
            .ReturnsAsync(Mock.Of<ItemResponse<TestDocument>>());

        // Act
        await _sut.PatchDocument("pk", "id", PatchOperation.Replace("/name", "New Name"));

        // Assert
        capturedPatches.Should().HaveCount(1);
        capturedPatches.Should().NotContain(p => p.Path == "/lastUpdatedBy");
    }
}
