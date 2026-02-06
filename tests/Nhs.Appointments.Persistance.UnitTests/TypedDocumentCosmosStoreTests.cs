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
            _lastUpdatedByResolver.Object);
    }
    
    [Fact]
    public void NotSupported_ContainerRetryOptions_NoCutoffOrInitial()
    {
        var unsupportedOptions = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db", ContainerRetryOptions =
            [
                new() { ContainerName = "test-container" }
            ]
        });
        
        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                unsupportedOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }
    
    [Fact]
    public void NotSupported_ContainerRetryOptions_NoInitialValue()
    {
        var unsupportedOptions = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db", ContainerRetryOptions =
            [
                new() { ContainerName = "test-container", CutoffRetryMs = 1000 }
            ]
        });
        
        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                unsupportedOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }
    
    [Fact]
    public void NotSupported_ContainerRetryOptions_NoCutoff()
    {
        var unsupportedOptions = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db", ContainerRetryOptions =
            [
                new() { ContainerName = "test-container", InitialValueMs = 150 }
            ]
        });
        
        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                unsupportedOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }
    
    [Fact]
    public void InvalidOperationException_ContainerRetryOptions_MultipleOptionsForSameContainer()
    {
        var unsupportedOptions = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db", ContainerRetryOptions =
            [
                new ContainerRetryOptions
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10000,
                    InitialValueMs = 100,
                },
                new ContainerRetryOptions
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Exponential,
                    CutoffRetryMs = 10000,
                    InitialValueMs = 5,
                }
            ]
        });
        
        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                unsupportedOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object);
        };
        ctor.Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void ContainerRetryOptions_ForDifferentContainer()
    {
        var unsupportedOptions = Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db", ContainerRetryOptions =
            [
                new() { ContainerName = "different-container" }
            ]
        });
        
        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                unsupportedOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object);
        };
        ctor.Should().NotThrow<NotSupportedException>();
    }

    [Fact]
    public async Task WriteAsync_LastUpdatedByIsUpdated()
    {
        // Arrange
        var testUser = "user@test.com";
        var doc = _sut.NewDocument();
        
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
    public async Task PatchDocument_IncludesLastUpdatedByOperation()
    {
        // Arrange
        var testUser = "patch-user@test.com";

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
}
