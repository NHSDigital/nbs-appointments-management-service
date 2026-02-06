using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Models;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Nhs.Appointments.Persistance;

public class TypedDocumentCosmosStore<TDocument> : ITypedDocumentCosmosStore<TDocument>
    where TDocument : TypedCosmosDocument, new()
{
    internal readonly string _containerName;
    private readonly ContainerRetryOptions  _containerRetryOptions;
    private readonly CosmosClient _cosmosClient;
    internal readonly string _databaseName;
    private readonly Lazy<string> _documentType;
    private readonly IMapper _mapper;
    private readonly IMetricsRecorder _metricsRecorder;
    private readonly ILastUpdatedByResolver _lastUpdatedByResolver;

    public TypedDocumentCosmosStore(
        CosmosClient cosmosClient, 
        IOptions<CosmosDataStoreOptions> options, 
        IMapper mapper,
        IMetricsRecorder metricsRecorder,
        ILastUpdatedByResolver lastUpdatedByResolver)
    {
        _cosmosClient = cosmosClient;
        _mapper = mapper;
        _documentType = new Lazy<string>(GetDocumentType);
        _databaseName = options.Value.DatabaseName;

        var cosmosDocumentAttribute = typeof(TDocument).GetCustomAttribute<CosmosDocumentAttribute>(true);
        if (cosmosDocumentAttribute == null)
        {
            throw new NotSupportedException("Document type must have a CosmosDocument attribute");
        }

        _containerName = cosmosDocumentAttribute.ContainerName;
        _containerRetryOptions = options.Value.ContainerRetryOptions.SingleOrDefault(x => x.ContainerName == _containerName);

        if (_containerRetryOptions != null && (_containerRetryOptions.InitialValueMs == 0 || _containerRetryOptions.CutoffRetryMs == 0))
        {
            throw new NotSupportedException("ContainerRetryOptions must have an InitialValueMs and CutoffRetryMs, if provided");
        }
        
        _metricsRecorder = metricsRecorder;
        _lastUpdatedByResolver = lastUpdatedByResolver;
    }

    private async Task<ItemResponse<T>> Retry_ItemResponse_OnTooManyRequests<T>(Func<Task<ItemResponse<T>>> operation, CancellationToken cancellationToken = default)
    {
        //don't retry if the container isn't configured for it
        if (_containerRetryOptions == null)
        {
            var result = await operation();
            RecordQueryMetrics(result.RequestCharge);
            return result;
        }
        
        var maxRetries = 5;
        var delay = 100;
        var backoffFactor = 2;

        var attempt = 1;

        ItemResponse<T> retryResult = null;
        
        //log metrics for total request charge for the initial attempt, and any retries required
        double totalRequestCharge = 0;
        
        while (attempt <= maxRetries)
        {
            try
            {
                if (attempt > 1)
                {
                    //TODO log a warning
                }
                
                retryResult = await operation();
            }
            catch (CosmosException ex)
            {
                totalRequestCharge += ex.RequestCharge;
                
                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay *= backoffFactor;
                    attempt++;
                }
                else
                {
                    RecordQueryMetrics(totalRequestCharge);
                    throw;
                }
            }
        }

        totalRequestCharge += retryResult!.RequestCharge;
        RecordQueryMetrics(totalRequestCharge);
        return retryResult;
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
        
        var result = await Retry_ItemResponse_OnTooManyRequests(() => GetContainer().UpsertItemAsync(document), CancellationToken.None);
    }

    public async Task<TModel> GetByIdAsync<TModel>(string documentId)
    {
        var readResponse = await Retry_ItemResponse_OnTooManyRequests(() => GetContainer().ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(_documentType.Value)), CancellationToken.None);
        
        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId)
    {
        return GetByIdOrDefaultAsync<TModel>(documentId, _documentType.Value);
    }

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId, string partitionKey)
    {
        var container = GetContainer();

        //TODO why is this done in this way, and not just GetByIdAsync??
        using (var response = await container.ReadItemStreamAsync(documentId, new PartitionKey(partitionKey)))
        {
            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var document = _cosmosClient.ClientOptions.Serializer.FromStream<TDocument>(response.Content);
            return _mapper.Map<TModel>(document);
        }
    }

    public Task<TModel> GetDocument<TModel>(string documentId)
    {
        return GetDocument<TModel>(documentId, _documentType.Value);
    }

    public async Task<TModel> GetDocument<TModel>(string documentId, string partitionKey)
    {
        var readResponse = await Retry_ItemResponse_OnTooManyRequests(() => GetContainer().ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)), CancellationToken.None);
        
        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        return IterateResults(queryFeed, rs => _mapper.Map<TModel>(rs));
    }

    public async Task DeleteDocument(string documentId, string partitionKey)
    {
        await Retry_ItemResponse_OnTooManyRequests(() => GetContainer().DeleteItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey)));
    }

    public Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return IterateResults(queryFeed, item => item);
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
        
        var result = await Retry_ItemResponse_OnTooManyRequests(() => GetContainer().PatchItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patchList.AsReadOnly()));

        return result.Resource;
    }

    public string GetDocumentType()
    {
        return typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;
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
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet.Select(map));
                requestCharge += resultSet.RequestCharge;
            }
        }

        RecordQueryMetrics(requestCharge);
        return results;
    }

    protected Container GetContainer() => _cosmosClient.GetContainer(_databaseName, _containerName);

    private void RecordQueryMetrics(double requestCharge)
    {
        _metricsRecorder.RecordMetric("RequestCharge", requestCharge);
    }
}
