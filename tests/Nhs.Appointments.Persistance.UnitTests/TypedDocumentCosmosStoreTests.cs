using System.Net;
using AutoMapper;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Persistance.UnitTests;

public class TypedDocumentCosmosStoreTests
{
    private readonly Mock<Container> _container = new();
    private readonly Mock<CosmosClient> _cosmosClient = new();
    private readonly Mock<ILastUpdatedByResolver> _lastUpdatedByResolver = new();

    private readonly Mock<ILogger<TestDocument>> _logger = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    private readonly IOptions<CosmosDataStoreOptions> _options =
        Options.Create(new CosmosDataStoreOptions { DatabaseName = "test-db" });

    private readonly IOptions<ContainerRetryOptions> _retryOptions =
        Options.Create(new ContainerRetryOptions { Configurations = [] });

    private readonly TypedDocumentCosmosStore<TestDocument> _sut;

    public TypedDocumentCosmosStoreTests()
    {
        _cosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(), It.IsAny<string>())).Returns(_container.Object);

        _sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            _retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Retry_ItemResponse_OnTooManyRequests__OperationInvokedOnlyOnceIfFirstInvocationSuccessful()
    {
        var noRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.GeometricDouble,
                    CutoffRetryMs = 200,
                    InitialValueMs = 10,
                }
            ]
        });

        var response = Mock.Of<ItemResponse<TestDocument>>(r =>
            r.Resource == new TestDocument { Id = "123" } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 2.5
        );

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();
        mockCosmosOperation.Setup(f => f()).ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            noRetryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var result =
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        result.Should().Be(response);
        mockCosmosOperation.Verify(f => f(), Times.Once);

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never
        );

        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 2.5), Times.Once);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__OperationInvokedOnlyOnceIfNoRetryRequiredForOperationInContainer()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions { Configurations = [] });

        var response = Mock.Of<ItemResponse<TestDocument>>(r =>
            r.Resource == new TestDocument { Id = "123" } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 1.5
        );

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var result =
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        result.Should().Be(response);
        mockCosmosOperation.Verify(f => f(), Times.Once);

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 1, for container: test-container, total delay time ms: 1000")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never
        );

        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 1.5), Times.Once);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__OperationInvokedTenTimesForDefaultConfiguration_OnAllThrown()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions { });

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            //initial attempt
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            //retries
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            //final attempt fails
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("Container 'test-container' too many requests were exceeded");
        
        //error for count exceeded
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests failed after max retries (9) exceeded for container")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        mockCosmosOperation.Verify(f => f(), Times.Exactly(10));

        //9 retries
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(9)
        );

        //ten invocations x 2
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 20), Times.Once);
    }
    
     [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__OperationsInvokedOnDefaultConfiguration_ExceedMaxTime()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions());

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            //initial attempt
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)))
            //retries
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)))
            //the 5th invocation exception has an expensive backoff for next attempt
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(30000)))
            //this is not invocated, as would have to wait > 30 secs before executing
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(100)));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("Container 'test-container' too many requests were exceeded");
        
        //error for cutoff exceeded
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests failed because the CutoffRetryMs (30000) would be exceeded on the next retry attempt : total retries: 5 for container: test-container, total delay time ms: 500")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        mockCosmosOperation.Verify(f => f(), Times.Exactly(6));
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(5)
        );

        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 12), Times.Once);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__OperationInvokedTenTimesForDefaultConfiguration_OnTenthSuccess()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions());

        var response = Mock.Of<ItemResponse<TestDocument>>(r =>
            r.Resource == new TestDocument { Id = "123" } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 2
        );

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            .ThrowsAsync(new RetryAfterCosmosException(TimeSpan.FromMilliseconds(50)))
            //final attempt succeeds
            .ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var result =
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        result.Should().Be(response);
        mockCosmosOperation.Verify(f => f(), Times.Exactly(10));

        //9 retries
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(9)
        );

        //ten invocations x 2
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 20), Times.Once);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__DefaultConfiguration_ThrowsException_IfNoRetryAfter()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions());

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            //RetryAfter prop not set
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);
        
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("TooManyRequests exception does not have a RetryAfter value");
    }
    
    [Fact]
    public async Task Retry_ItemResponse_OnTooManyRequests__OperationInvokedTwiceIfASingleRetryRequiredForContainer()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 1000,
                    InitialValueMs = 100,
                }
            ]
        });

        var response = Mock.Of<ItemResponse<TestDocument>>(r =>
            r.Resource == new TestDocument { Id = "123" } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 3.5
        );

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var result =
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        result.Should().Be(response);
        mockCosmosOperation.Verify(f => f(), Times.Exactly(2));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 1, for container: test-container, total delay time ms: 100")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        //total of initial 2 exception, and 3.5 successful retry
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 5.5), Times.Once);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__OperationInvoked_NPlus1_TimesIf_N_RetriesRequiredForContainer(
            int retriesNeeded)
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10,
                    InitialValueMs = 1,
                }
            ]
        });

        var response = Mock.Of<ItemResponse<TestDocument>>(r =>
            r.Resource == new TestDocument { Id = "123" } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 3.5
        );

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        var chain = mockCosmosOperation
            .SetupSequence(f => f());

        for (var i = 0; i < retriesNeeded; i++)
        {
            chain.ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));
        }

        //finally a success
        chain.ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var result =
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        result.Should().Be(response);
        mockCosmosOperation.Verify(f => f(), Times.Exactly(retriesNeeded + 1));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(retriesNeeded)
        );

        //each exception = 2 x retires plus 3.5 successful
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", (2 * retriesNeeded) + 3.5), Times.Once);
    }


    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public async Task
        Retry_FeedResponse_TDocument_OnTooManyRequests__OperationInvoked_NPlus1_TimesIf_N_RetriesRequiredForContainer(
            int retriesNeeded)
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10,
                    InitialValueMs = 1,
                }
            ]
        });

        var response = Mock.Of<FeedResponse<TestDocument>>(r =>
            r.Resource == new List<TestDocument>
            {
                new() { Id = "123" }, new() { Id = "456" }, new() { Id = "789" },
            } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 3.5
        );

        var mockCosmosOperation = new Mock<Func<Task<FeedResponse<TestDocument>>>>();

        var chain = mockCosmosOperation
            .SetupSequence(f => f());

        for (var i = 0; i < retriesNeeded; i++)
        {
            chain.ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));
        }

        //finally a success
        chain.ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None);

        mockCosmosOperation.Verify(f => f(), Times.Exactly(retriesNeeded + 1));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(retriesNeeded)
        );

        //each exception = 2 x retires plus 3.5 successful
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", (2 * retriesNeeded) + 3.5), Times.Once);
    }

    [Fact]
    public async Task Retry_FeedResponse_TModel_OnTooManyRequests__ThrowsInvalidOperation_IfCanExtractTrue()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10,
                    InitialValueMs = 1,
                }
            ]
        });

        var response = Mock.Of<FeedResponse<TestModel>>(r =>
            r.Resource == new List<TestModel> { new() { Id = "123" }, new() { Id = "456" }, new() { Id = "789" }, } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 3.5
        );

        var mockCosmosOperation = new Mock<Func<Task<FeedResponse<TestModel>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        //invalid usage
        var action = async () =>
        {
            _ = await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None,
                true);
        };
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public async Task
        Retry_FeedResponse_TModel_OnTooManyRequests__OperationInvoked_NPlus1_TimesIf_N_RetriesRequiredForContainer(
            int retriesNeeded)
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10,
                    InitialValueMs = 1,
                }
            ]
        });

        var response = Mock.Of<FeedResponse<TestModel>>(r =>
            r.Resource == new List<TestModel> { new() { Id = "123" }, new() { Id = "456" }, new() { Id = "789" }, } &&
            r.StatusCode == HttpStatusCode.OK &&
            r.RequestCharge == 3.5
        );

        var mockCosmosOperation = new Mock<Func<Task<FeedResponse<TestModel>>>>();

        var chain = mockCosmosOperation
            .SetupSequence(f => f());

        for (var i = 0; i < retriesNeeded; i++)
        {
            chain.ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));
        }

        //finally a success
        chain.ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None, false);

        mockCosmosOperation.Verify(f => f(), Times.Exactly(retriesNeeded + 1));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(retriesNeeded)
        );

        //cannot extract request charge from TModel usage of FeedResponse
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", It.IsAny<double>()), Times.Never);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public async Task
        Retry_ResponseMessage_OnTooManyRequests__OperationInvoked_NPlus1_TimesIf_N_RetriesRequiredForContainer(
            int retriesNeeded)
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10,
                    InitialValueMs = 1,
                }
            ]
        });

        var response = Mock.Of<ResponseMessage>(r =>
            r.StatusCode == HttpStatusCode.OK
        );

        var mockCosmosOperation = new Mock<Func<Task<ResponseMessage>>>();

        var chain = mockCosmosOperation
            .SetupSequence(f => f());

        for (var i = 0; i < retriesNeeded; i++)
        {
            chain.ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));
        }

        //finally a success
        chain.ReturnsAsync(response);

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None,
            canExtractRequestCharge: false);

        mockCosmosOperation.Verify(f => f(), Times.Exactly(retriesNeeded + 1));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Cosmos TooManyRequests retryCount")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Exactly(retriesNeeded)
        );

        //Response Message does not record metrics
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", It.IsAny<double>()), Times.Never);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__ErrorOutIfTooManyRetriesRequiredForContainer__LinearBackoff()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 500,
                    InitialValueMs = 100,
                }
            ]
        });

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("Container 'test-container' too many requests were exceeded");
        
        //error for cutoff exceeded
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests failed because the CutoffRetryMs (500) would be exceeded on the next retry attempt : total retries: 5 for container: test-container, total delay time ms: 500")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        //initial attempt, plus 5 retries
        mockCosmosOperation.Verify(f => f(), Times.Exactly(6));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 1, for container: test-container, total delay time ms: 100")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 2, for container: test-container, total delay time ms: 200")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 3, for container: test-container, total delay time ms: 300")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 4, for container: test-container, total delay time ms: 400")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 5, for container: test-container, total delay time ms: 500")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        //6x2
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 12), Times.Once);
    }

    [Fact]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__ErrorOutIfTooManyRetriesRequiredForContainer__GeometricBackoff()
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.GeometricDouble,
                    CutoffRetryMs = 200,
                    InitialValueMs = 10,
                }
            ]
        });

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            //initial call
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2)) //10
            //retries
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2)) //20
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2)) //40
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2)) //80
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2)) //160
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("Container 'test-container' too many requests were exceeded");
        
        //error for cutoff exceeded
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests failed because the CutoffRetryMs (200) would be exceeded on the next retry attempt : total retries: 4 for container: test-container, total delay time ms: 150")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        //initial attempt, plus 4 retries
        mockCosmosOperation.Verify(f => f(), Times.Exactly(5));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 1, for container: test-container, total delay time ms: 10")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 2, for container: test-container, total delay time ms: 30")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 3, for container: test-container, total delay time ms: 70")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            "Cosmos TooManyRequests retryCount: 4, for container: test-container, total delay time ms: 150")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        //5x2
        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 10), Times.Once);
    }

    [Theory]
    [InlineData(13, 35, 96, 261, 600)]
    [InlineData(10, 27, 73, 200, 500)]
    [InlineData(7, 19, 51, 140, 350)]
    [InlineData(5, 13, 36, 100, 250)]
    public async Task
        Retry_ItemResponse_OnTooManyRequests__ErrorOutIfTooManyRetriesRequiredForContainer__ExponentialBackoff(
            int initialValue, int expectedSecondValue, int expectedThirdValue, int expectedFourthValue, int cutoff)
    {
        var retryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new ContainerRetryConfiguration
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Exponential,
                    CutoffRetryMs = cutoff,
                    InitialValueMs = initialValue,
                }
            ]
        });

        var mockCosmosOperation = new Mock<Func<Task<ItemResponse<TestDocument>>>>();

        mockCosmosOperation
            .SetupSequence(f => f())
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2))
            .ThrowsAsync(new CosmosException("Boom", HttpStatusCode.TooManyRequests, 0, "", 2));

        var sut = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            retryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.Retry_CosmosOperation_OnTooManyRequests(mockCosmosOperation.Object, CancellationToken.None));

        exception.Message.Should().Contain("Container 'test-container' too many requests were exceeded");
        
        //error for cutoff exceeded
        _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests failed because the CutoffRetryMs ({cutoff}) would be exceeded on the next retry attempt : total retries: 4 for container: test-container, total delay time ms: {initialValue + expectedSecondValue + expectedThirdValue + expectedFourthValue}")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        mockCosmosOperation.Verify(f => f(), Times.Exactly(5));

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests retryCount: 1, for container: test-container, total delay time ms: {initialValue}")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests retryCount: 2, for container: test-container, total delay time ms: {initialValue + expectedSecondValue}")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests retryCount: 3, for container: test-container, total delay time ms: {initialValue + expectedSecondValue + expectedThirdValue}")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString()
                        .Contains(
                            $"Cosmos TooManyRequests retryCount: 4, for container: test-container, total delay time ms: {initialValue + expectedSecondValue + expectedThirdValue + expectedFourthValue}")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );

        _metricsRecorder.Verify(f => f.RecordMetric("RequestCharge", 10), Times.Once);
    }

    [Fact]
    public void Valid_ContainerRetryOptions_ForTestDocumentContainer()
    {
        var supportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new()
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10000,
                    InitialValueMs = 100,
                }
            ]
        });

        var ctor = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            supportedRetryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        ctor.ContainerRetryConfiguration.Should().BeEquivalentTo(supportedRetryOptions.Value.Configurations.Single());
    }

    [Fact]
    public void NotSupported_ContainerRetryOptions_NoCutoffOrInitial()
    {
        var unsupportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new() { ContainerName = "test-container" }
            ]
        });

        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                _options,
                unsupportedRetryOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object,
                _logger.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void NotSupported_ContainerRetryOptions_NoInitialValue()
    {
        var unsupportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new() { ContainerName = "test-container", CutoffRetryMs = 1000 },
            ]
        });

        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                _options,
                unsupportedRetryOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object,
                _logger.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void NotSupported_ContainerRetryOptions_NoCutoff()
    {
        var unsupportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new() { ContainerName = "test-container", InitialValueMs = 150 },
            ]
        });

        var ctor = () =>
        {
            _ = new TypedDocumentCosmosStore<TestDocument>(
                _cosmosClient.Object,
                _options,
                unsupportedRetryOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object,
                _logger.Object);
        };
        ctor.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void InvalidOperationException_ContainerRetryOptions_MultipleOptionsForSameContainer()
    {
        var unsupportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new()
                {
                    ContainerName = "test-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10000,
                    InitialValueMs = 100,
                },
                new()
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
                _options,
                unsupportedRetryOptions,
                _mapper.Object,
                _metricsRecorder.Object,
                _lastUpdatedByResolver.Object,
                _logger.Object);
        };
        ctor.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ContainerRetryOptions_ForDifferentContainer_UsesDefault()
    {
        var unsupportedRetryOptions = Options.Create(new ContainerRetryOptions
        {
            Configurations =
            [
                new()
                {
                    ContainerName = "different-container",
                    BackoffRetryType = BackoffRetryType.Linear,
                    CutoffRetryMs = 10000,
                    InitialValueMs = 100,
                }
            ]
        });

        var ctor = new TypedDocumentCosmosStore<TestDocument>(
            _cosmosClient.Object,
            _options,
            unsupportedRetryOptions,
            _mapper.Object,
            _metricsRecorder.Object,
            _lastUpdatedByResolver.Object,
            _logger.Object);

        //config for different container supplied (not test document container)
        //falls back to default
        ctor.ContainerRetryConfiguration.Should().BeEquivalentTo(new ContainerRetryConfiguration()
        {
            BackoffRetryType = BackoffRetryType.CosmosDefault,
            ContainerName = "test-container",
            CutoffRetryMs = 30000
        });
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
            .Callback<string, PartitionKey, IReadOnlyList<PatchOperation>, PatchItemRequestOptions,
                CancellationToken>((id, pk, patches, opts, ct) => capturedPatches = patches)
            .ReturnsAsync(Mock.Of<ItemResponse<TestDocument>>());

        // Act
        await _sut.PatchDocument("pk", "id", PatchOperation.Replace("/name", "New Name"));

        // Assert
        capturedPatches.Should().HaveCount(2);
        capturedPatches.Should().ContainSingle(p =>
            p.OperationType == PatchOperationType.Set &&
            p.Path == "/lastUpdatedBy");
    }
    
    private sealed class RetryAfterCosmosException(TimeSpan retryAfter) : CosmosException("Boom",
        HttpStatusCode.TooManyRequests, subStatusCode: 0,
        activityId: Guid.NewGuid().ToString(), requestCharge: 2)
    {
        public override TimeSpan? RetryAfter => retryAfter;
    }
}
