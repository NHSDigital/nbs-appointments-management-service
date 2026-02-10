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
            retryOptions?.Value?.Configurations.SingleOrDefault(x => x.ContainerName == ContainerName);

        if (ContainerRetryConfiguration != null && (ContainerRetryConfiguration.InitialValueMs == 0 ||
                                                    ContainerRetryConfiguration.CutoffRetryMs == 0))
        {
            throw new NotSupportedException(
                "ContainerRetryOptions must have an InitialValueMs and CutoffRetryMs, if provided");
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

        await Retry_ItemResponse_OnTooManyRequests(async () => await GetContainer().UpsertItemAsync(document),
            CancellationToken.None);
    }

    public async Task<TModel> GetByIdAsync<TModel>(string documentId)
    {
        var readResponse = await Retry_ItemResponse_OnTooManyRequests(async () => await GetContainer().ReadItemAsync<TDocument>(
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
        using var response = await Retry_ResponseMessage_OnTooManyRequests(async () => await GetContainer().ReadItemStreamAsync(documentId, new PartitionKey(partitionKey)), CancellationToken.None);
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
        var readResponse = await Retry_ItemResponse_OnTooManyRequests(async () => await GetContainer().ReadItemAsync<TDocument>(
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
        await Retry_ItemResponse_OnTooManyRequests(async () => await GetContainer().DeleteItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)));
    }

    public async Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return await IterateResults(queryFeed, item => item);
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

        var result = await Retry_ItemResponse_OnTooManyRequests(async () => await GetContainer().PatchItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patchList.AsReadOnly()));

        return result.Resource;
    }

    public string GetDocumentType()
    {
        return typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;
    }

    internal async Task<ItemResponse<T>> Retry_ItemResponse_OnTooManyRequests<T>(
        Func<Task<ItemResponse<T>>> cosmosOperation, CancellationToken cancellationToken = default)
    {
        //don't retry if the container isn't configured for it
        if (ContainerRetryConfiguration == null)
        {
            var result = await cosmosOperation();
            RecordQueryMetrics(result.RequestCharge);
            return result;
        }

        var attemptRequired = true;
        var attemptCount = 1;

        var linkId = Guid.NewGuid();

        var delayMs = ContainerRetryConfiguration.InitialValueMs;
        var totalDelayMs = 0;
        ItemResponse<T> retryResult = null;
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

        totalRequestCharge += retryResult!.RequestCharge;
        RecordQueryMetrics(totalRequestCharge);

        return retryResult;
    }

    internal async Task<FeedResponse<T>> Retry_FeedResponse_OnTooManyRequests<T>(
        Func<Task<FeedResponse<T>>> cosmosOperation, CancellationToken cancellationToken = default)
    {
        //don't retry if the container isn't configured for it
        if (ContainerRetryConfiguration == null)
        {
            var result = await cosmosOperation();
            RecordQueryMetrics(result.RequestCharge);
            return result;
        }

        var attemptRequired = true;
        var attemptCount = 1;

        var linkId = Guid.NewGuid();

        var delayMs = ContainerRetryConfiguration.InitialValueMs;
        var totalDelayMs = 0;
        FeedResponse<T> retryResult = null;
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

        totalRequestCharge += retryResult!.RequestCharge;
        RecordQueryMetrics(totalRequestCharge);

        return retryResult;
    }
    
     internal async Task<ResponseMessage> Retry_ResponseMessage_OnTooManyRequests(
        Func<Task<ResponseMessage>> cosmosOperation, CancellationToken cancellationToken = default)
    {
        //don't retry if the container isn't configured for it
        if (ContainerRetryConfiguration == null)
        {
            var result = await cosmosOperation();
            return result;
        }

        var attemptRequired = true;
        var attemptCount = 1;

        var linkId = Guid.NewGuid();

        var delayMs = ContainerRetryConfiguration.InitialValueMs;
        var totalDelayMs = 0;
        ResponseMessage retryResult = null;
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

        return retryResult;
    }
    
    private async Task<IEnumerable<TOutput>> IterateResults<TSource, TOutput>(FeedIterator<TSource> queryFeed,
        Func<TSource, TOutput> map)
    {
        var requestCharge = 0.0;
        var results = new List<TOutput>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await Retry_FeedResponse_OnTooManyRequests(async () => await queryFeed.ReadNextAsync(), CancellationToken.None);
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
