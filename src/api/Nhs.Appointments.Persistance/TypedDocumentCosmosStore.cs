using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class TypedDocumentCosmosStore<TDocument> : ITypedDocumentCosmosStore<TDocument>
    where TDocument : TypedCosmosDocument, new()
{
    private readonly CosmosClient _cosmosClient;
    private readonly Lazy<string> _documentType;
    private readonly ILastUpdatedByResolver _lastUpdatedByResolver;
    private readonly ILogger<TDocument> _logger;
    private readonly IMapper _mapper;
    private readonly IMetricsRecorder _metricsRecorder;
    internal readonly string ContainerName;
    internal readonly ContainerRetryConfiguration ContainerRetryConfiguration;
    internal readonly string DatabaseName;

    public TypedDocumentCosmosStore(
        CosmosClient cosmosClient,
        IOptions<CosmosDataStoreOptions> options,
        IOptions<ContainerRetryOptions> retryOptions,
        IMapper mapper,
        IMetricsRecorder metricsRecorder,
        ILastUpdatedByResolver lastUpdatedByResolver,
        ILogger<TDocument> logger)
    {
        _cosmosClient = cosmosClient;
        _mapper = mapper;
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

        if (ContainerRetryConfiguration != null && (ContainerRetryConfiguration.InitialValueMs == 0 ||
                                                    ContainerRetryConfiguration.CutoffRetryMs == 0))
        {
            throw new NotSupportedException(
                "ContainerRetryOptions must have an InitialValueMs and CutoffRetryMs, if provided");
        }

        //apply default to simulate default cosmos retry options
        if (ContainerRetryConfiguration == null)
        {
            //this is equivalent to a simple doubling backoff in our custom implementation
            //where the 9th retry is allowed, but the cumulative cutoff is hit preventing a 10th retry attempt
            ContainerRetryConfiguration = new ContainerRetryConfiguration
            {
                
                ContainerName = ContainerName, 
                //default cosmos max cumulative wait is 30 seconds.
                CutoffRetryMs = 30000,
                
                //default cosmos max retries = 9,
                //these two values are both reverse engineered so that this behaviour occurs
                InitialValueMs = 50, 
                BackoffRetryType = BackoffRetryType.GeometricDouble
            };
        }

        _metricsRecorder = metricsRecorder;
        _lastUpdatedByResolver = lastUpdatedByResolver;
    }

    public TDocument NewDocument()
    {
        var document = new TDocument();
        document.DocumentType = _documentType.Value;
        return document;
    }

    public TDocument ConvertToDocument<TModel>(TModel model)
    {
        var document = _mapper.Map<TDocument>(model);
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

        await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().UpsertItemAsync(document),
            CancellationToken.None);
    }

    public async Task<TModel> GetByIdAsync<TModel>(string documentId)
    {
        var readResponse = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(_documentType.Value)), CancellationToken.None);

        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId)
    {
        return await GetByIdOrDefaultAsync<TModel>(documentId, _documentType.Value);
    }

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId, string partitionKey)
    {
        using var response = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().ReadItemStreamAsync(documentId, new PartitionKey(partitionKey)), CancellationToken.None, canExtractRequestCharge: false);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        var document = _cosmosClient.ClientOptions.Serializer.FromStream<TDocument>(response.Content);
        return _mapper.Map<TModel>(document);
    }

    public async Task<TModel> GetDocument<TModel>(string documentId)
    {
        return await GetDocument<TModel>(documentId, _documentType.Value);
    }

    public async Task<TModel> GetDocument<TModel>(string documentId, string partitionKey)
    {
        var readResponse = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)), CancellationToken.None);

        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public async Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        return await IterateResults(queryFeed, rs => _mapper.Map<TModel>(rs));
    }

    public async Task DeleteDocument(string documentId, string partitionKey)
    {
        await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().DeleteItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)));
    }

    public async Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return await IterateResults(queryFeed, item => item, canExtractRequestCharge: false);
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

        var result = await Retry_CosmosOperation_OnTooManyRequests(async () => await GetContainer().PatchItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patchList.AsReadOnly()));

        return result.Resource;
    }

    public string GetDocumentType()
    {
        return typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;
    }
     
     /// <summary>
     /// Provides custom retry and backoff logic for the Cosmos TooManyRequests exception support. <br />
     /// This is configured per container, provided by the ContainerRetryConfiguration options. <br />
     /// If the container this operation targets has no provided ContainerRetryConfiguration, it will attempt the operation once with no retries, as the default behaviour.<br />
     /// </summary>
     /// <param name="cosmosOperation">The action to perform on the Cosmos DB</param>
     /// <param name="cancellationToken">Cancellation token for the Task.Delay</param>
     /// <param name="canExtractRequestCharge">Whether the generic 'T' inherits Microsoft.Azure.Cosmos.Response type using TDocument. This allows the recording of a request charge, if so.</param>
     /// <typeparam name="T">Generic Cosmos return type for the generic document</typeparam>
     /// <returns></returns>
     /// <exception cref="ArgumentOutOfRangeException">When BackoffRetryType configuration is not supported</exception>
     /// <exception cref="InvalidOperationException">When the totaled retry delay times exceed the allowed cutoff time window</exception>
     internal async Task<T> Retry_CosmosOperation_OnTooManyRequests<T>(
        Func<Task<T>> cosmosOperation, CancellationToken cancellationToken = default, bool canExtractRequestCharge = true)
    {
        //don't retry if the container isn't configured for it
        if (ContainerRetryConfiguration == null)
        {
            var result = await cosmosOperation();

            if (canExtractRequestCharge)
            {
                RecordQueryMetrics(ExtractRequestCharge(result));
            }
            
            return result;
        }

        var attemptRequired = true;
        var attemptCount = 1;

        var linkId = Guid.NewGuid();

        var delayMs = ContainerRetryConfiguration.InitialValueMs;
        var totalDelayMs = 0;
        var retryResult = default(T);
        //log metrics for total request charge for the initial attempt, and any retries required
        double totalRequestCharge = 0;

        //used for exponential only
        double exponent = 0;

        switch (ContainerRetryConfiguration.BackoffRetryType)
        {
            case BackoffRetryType.Linear:
            case BackoffRetryType.GeometricDouble:
                //no work to do
                break;
            case BackoffRetryType.Exponential:
                //derive initial exponent needed to increment next delays, using the provided initial value
                exponent = Math.Log(ContainerRetryConfiguration.InitialValueMs);
                
                //increment for next usage
                exponent++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        while (attemptRequired && totalDelayMs <= ContainerRetryConfiguration.CutoffRetryMs)
        {
            try
            {
                if (attemptCount > 1)
                {
                    _logger.LogInformation(
                        "{linkId} - Cosmos TooManyRequests retryCount: {retryCount}, for container: {container}, total delay time: {totalDelayMs}",
                        linkId, attemptCount - 1, ContainerName, totalDelayMs);
                }

                retryResult = await cosmosOperation();

                //if we get to here, there wasn't a cosmos exception, so no need to retry
                attemptRequired = false;
            }
            catch (CosmosException ex)
            {
                totalRequestCharge += ex.RequestCharge;

                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    attemptCount++;

                    await Task.Delay(delayMs, cancellationToken);

                    //keep track of total delay time for this cosmosOperation
                    totalDelayMs += delayMs;

                    switch (ContainerRetryConfiguration.BackoffRetryType)
                    {
                        case BackoffRetryType.Linear:
                            //do nothing, next delay stays as same for each iteration
                            break;
                        case BackoffRetryType.Exponential:
                            //increment the exponent to derive next delay value needed for exponential backoff
                            delayMs = (int)Math.Floor(Math.Exp(exponent++));
                            break;
                        case BackoffRetryType.GeometricDouble:
                            //double delay time between retries
                            delayMs *= 2;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    RecordQueryMetrics(totalRequestCharge);
                    throw;
                }
            }
        }

        if (totalDelayMs > ContainerRetryConfiguration.CutoffRetryMs)
        {
            RecordQueryMetrics(totalRequestCharge);
            
            _logger.LogError(
                "{linkId} - Cosmos TooManyRequests failed after CutoffRetryMs surpassed: {CutoffRetryMs}, with retryCount: {retryCount} for container: {container}, total delay time: {totalDelayMs}",
                linkId, ContainerRetryConfiguration.CutoffRetryMs, attemptCount, ContainerName, totalDelayMs);
            throw new InvalidOperationException(
                $"Container '{ContainerName}' too many requests were exceeded for linkId: {linkId}");
        }

        if (canExtractRequestCharge)
        {
            totalRequestCharge += ExtractRequestCharge(retryResult);
            RecordQueryMetrics(totalRequestCharge);
        }

        return retryResult;
    }

    private double ExtractRequestCharge<T>(T result)
    {
        return result switch
        {
            Response<TDocument> itemResponse => itemResponse.RequestCharge,
            Response<IEnumerable<TDocument>> listResponse => listResponse.RequestCharge,
            _ => throw new InvalidOperationException()
        };
    }

    private async Task<IEnumerable<TOutput>> IterateResults<TSource, TOutput>(FeedIterator<TSource> queryFeed,
        Func<TSource, TOutput> map, bool canExtractRequestCharge = true)
    {
        var requestCharge = 0.0;
        var results = new List<TOutput>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await Retry_CosmosOperation_OnTooManyRequests(async () => await queryFeed.ReadNextAsync(), CancellationToken.None, canExtractRequestCharge);
                results.AddRange(resultSet.Select(map));
                requestCharge += resultSet.RequestCharge;
            }
        }

        RecordQueryMetrics(requestCharge);
        return results;
    }

    protected Container GetContainer() => _cosmosClient.GetContainer(DatabaseName, ContainerName);

    private void RecordQueryMetrics(double requestCharge)
    {
        _metricsRecorder.RecordMetric("RequestCharge", requestCharge);
    }
}
