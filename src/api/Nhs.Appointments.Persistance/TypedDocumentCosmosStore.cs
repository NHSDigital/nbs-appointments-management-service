using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class TypedDocumentCosmosStore<TDocument> : ITypedDocumentCosmosStore<TDocument>
    where TDocument : TypedCosmosDocument, new()
{
    private readonly CosmosClient _cosmosClient;
    private readonly Lazy<string> _documentType;
    private readonly ILastUpdatedByResolver _lastUpdatedByResolver;
    private readonly ILogger<TDocument> _logger;
    private readonly IMetricsRecorder _metricsRecorder;
    internal readonly string ContainerName;
    internal readonly ContainerRetryConfiguration ContainerRetryConfiguration;
    internal readonly string DatabaseName;

    public TypedDocumentCosmosStore(
        CosmosClient cosmosClient,
        IOptions<CosmosDataStoreOptions> options,
        IOptions<ContainerRetryOptions> retryOptions,
        IMetricsRecorder metricsRecorder,
        ILastUpdatedByResolver lastUpdatedByResolver,
        ILogger<TDocument> logger)
    {
        _cosmosClient = cosmosClient;
        _documentType = new Lazy<string>(GetDocumentType);
        DatabaseName = options.Value.DatabaseName;
        _logger = logger;

        var cosmosDocumentAttribute = typeof(TDocument).GetCustomAttribute<CosmosDocumentAttribute>(true);
        if (cosmosDocumentAttribute == null)
        {
            throw new NotSupportedException("Document type must have a CosmosDocument attribute");
        }

        ContainerName = cosmosDocumentAttribute.ContainerName;
        ContainerRetryConfiguration =
            retryOptions?.Value?.Configurations?.SingleOrDefault(x => x.ContainerName == ContainerName);

        if (ContainerRetryConfiguration != null)
        {
            if (ContainerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault)
            {
                throw new NotSupportedException(
                    "ContainerRetryOptions must have a non-default backoff retry type, if provided. If the default cosmos behaviour is desired, remove the ContainerRetryOptions for this container");
            }

            if (ContainerRetryConfiguration.InitialValueMs == 0 ||
                ContainerRetryConfiguration.CutoffRetryMs == 0)
            {
                throw new NotSupportedException(
                    "ContainerRetryOptions must have an InitialValueMs and CutoffRetryMs, if provided");
            }
        }

        //default cosmos behaviour
        ContainerRetryConfiguration ??= new ContainerRetryConfiguration
        {
            ContainerName = ContainerName,
            BackoffRetryType = BackoffRetryType.CosmosDefault,
            CutoffRetryMs = DefaultCosmosCutoffRetryMs
        };

        _metricsRecorder = metricsRecorder;
        _lastUpdatedByResolver = lastUpdatedByResolver;
    }

    private const int DefaultCosmosCutoffRetryMs = 30000;

    public TDocument NewDocument()
    {
        var document = new TDocument();
        document.DocumentType = _documentType.Value;
        return document;
    }

    public async Task WriteAsync(TDocument document)
    {
        if (document.DocumentType != _documentType.Value)
        {
            throw new InvalidOperationException("Document type does not match the supported type for this writer");
        }

        if (document is LastUpdatedByCosmosDocument auditable)
        {
            auditable.LastUpdatedBy = _lastUpdatedByResolver.GetLastUpdatedBy();
        }

        await Retry_CosmosOperation_OnTooManyRequests(async () =>
            {
                //if the operation is retried, want to regenerate the new updatedOn time.
                document.LastUpdatedOn = DateTime.UtcNow;
                return await GetContainer().UpsertItemAsync(document);
            },
            CancellationToken.None);
    }

    public async Task<TDocument> GetByIdAsync(string documentId)
    {
        var readResponse = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer()
            .ReadItemAsync<TDocument>(
                id: documentId,
                partitionKey: new PartitionKey(_documentType.Value)),
            nameof(GetByIdAsync),
            CancellationToken.None
        );

        return readResponse.Resource;
    }

    public async Task<TDocument> GetByIdOrDefaultAsync(string documentId)
    {
        return await GetByIdOrDefaultAsync(documentId, _documentType.Value);
    }

    public async Task<TDocument> GetByIdOrDefaultAsync(string documentId, string partitionKey)
    {
        using var response = await Retry_CosmosOperation_OnTooManyRequests(
            async () => await GetContainer().ReadItemStreamAsync(documentId, new PartitionKey(partitionKey)),
            nameof(GetByIdOrDefaultAsync),
            CancellationToken.None
        );
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return _cosmosClient.ClientOptions.Serializer.FromStream<TDocument>(response.Content);
    }

    public async Task<TDocument> GetDocument(string documentId)
    {
        return await GetDocument(documentId, _documentType.Value);
    }

    public async Task<TDocument> GetDocument(string documentId, string partitionKey)
    {
        var readResponse = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer()
            .ReadItemAsync<TDocument>(
                id: documentId,
                partitionKey: new PartitionKey(partitionKey)),
            nameof(GetDocument),
            CancellationToken.None
        );

        return readResponse.Resource;
    }

    public async Task<IEnumerable<TDocument>> RunQueryAsync(Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        return await IterateResults(queryFeed);
    }

    public async Task DeleteDocument(string documentId, string partitionKey)
    {
        await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().DeleteItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)),
            nameof(DeleteDocument)
            );
    }

    public async Task<IEnumerable<TDocument>> RunSqlQueryAsync(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TDocument>(
            queryDefinition: query);

        return await IterateResults(queryFeed, canExtractRequestCharge: false);
    }

    public async Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return await IterateResults(queryFeed, canExtractRequestCharge: false);
    }

    public async Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches)
    {
        var patchList = patches.ToList();

        if (typeof(LastUpdatedByCosmosDocument).IsAssignableFrom(typeof(TDocument)))
        {
            patchList.Add(PatchOperation.Set("/lastUpdatedBy", _lastUpdatedByResolver.GetLastUpdatedBy()));
        }

        if (patchList.Count > 10)
        {
            throw new NotSupportedException("Only 10 patches can be applied");
        }

        patchList.Add(PatchOperation.Set("/lastUpdatedOn", DateTime.UtcNow));

        var result = await Retry_CosmosOperation_OnTooManyRequests(async () =>
        {
            //remove and readd to force readOnly list to update value
            var lastUpdatedOnPatch = patchList.Single(x => x.Path == "/lastUpdatedOn");
            patchList.Remove(lastUpdatedOnPatch);
            patchList.Add(PatchOperation.Set("/lastUpdatedOn", DateTime.UtcNow));
            
            return await GetContainer()
                .PatchItemAsync<TDocument>(
                    id: documentId,
                    partitionKey: new PartitionKey(partitionKey),
                    patchOperations: patchList);
        });

        return result.Resource;
    }

    public string GetDocumentType()
    {
        return typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;
    }

    /// <summary>
    ///     Provides custom retry and backoff logic for the Cosmos TooManyRequests exception support. <br />
    ///     This is configured per container, provided by the ContainerRetryConfiguration options. <br />
    ///     If the container this operation targets has no provided ContainerRetryConfiguration, it will attempt the operation
    ///     once with no retries, as the default behaviour.<br />
    /// </summary>
    /// <param name="cosmosOperation">The action to perform on the Cosmos DB</param>
    /// <param name="cancellationToken">Cancellation token for the Task.Delay</param>
    /// <param name="canExtractRequestCharge">
    ///     Whether the generic 'T' inherits Microsoft.Azure.Cosmos.Response type using
    ///     TDocument. This allows the recording of a request charge, if so.
    /// </param>
    /// <typeparam name="T">Generic Cosmos return type for the generic document</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">When BackoffRetryType configuration is not supported</exception>
    /// <exception cref="InvalidOperationException">When the totaled retry delay times exceed the allowed cutoff time window</exception>
    internal async Task<T> Retry_CosmosOperation_OnTooManyRequests<T>(
        Func<Task<T>> cosmosOperation,
        string path,
        CancellationToken cancellationToken = default,
        bool canExtractRequestCharge = true)
    {
        var metric = new CosmosOperationMetric
        {
            Container = ContainerName,
            DocumentType = _documentType.Value,
            Path = path
        };

        var result = await CosmosOperationHelper.Retry_CosmosOperation_OnTooManyRequests(
            ContainerRetryConfiguration,
            cosmosOperation,
            _logger,
            _metricsRecorder,
            metric,
            cancellationToken);

        if (canExtractRequestCharge)
        {
            metric.AddRuCharge(ExtractRequestCharge(result));
        }
        CosmosOperationHelper.RecordQueryMetrics(_metricsRecorder, metric);

        return result;
    }

    private static double ExtractRequestCharge<T>(T result)
    {
        return result switch
        {
            Response<TDocument> itemResponse => itemResponse.RequestCharge,
            Response<IEnumerable<TDocument>> listResponse => listResponse.RequestCharge,
            ResponseMessage responseMessage => responseMessage.Headers.RequestCharge,
            _ => throw new InvalidOperationException()
        };
    }

    private async Task<IEnumerable<TSource>> IterateResults<TSource>(FeedIterator<TSource> queryFeed, bool canExtractRequestCharge = true)
    {
        var requestCharge = 0.0;
        var results = new List<TSource>();

        var metric = new CosmosOperationMetric
        {
            Container = ContainerName,
            DocumentType = _documentType.Value,
            Path = path
        };
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await CosmosOperationHelper.Retry_CosmosOperation_OnTooManyRequests(
                    ContainerRetryConfiguration,
                    async () => await queryFeed.ReadNextAsync(),
                    _logger,
                    _metricsRecorder,
                    metric,
                    CancellationToken.None);

                results.AddRange(resultSet.Select(r => r));
                metric.AddRuCharge(resultSet.RequestCharge);
            }
        }

        CosmosOperationHelper.RecordQueryMetrics(_metricsRecorder, metric);

        return results;
    }

    protected Container GetContainer() => _cosmosClient.GetContainer(DatabaseName, ContainerName);
}
