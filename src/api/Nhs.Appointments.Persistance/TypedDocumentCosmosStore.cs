using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class TypedDocumentCosmosStore<TDocument> : ITypedDocumentCosmosStore<TDocument>
    where TDocument : TypedCosmosDocument, new()
{
    internal readonly string _containerName;
    private readonly CosmosClient _cosmosClient;
    internal readonly string _databaseName;
    private readonly Lazy<string> _documentType;
    private readonly IMapper _mapper;
    private readonly IMetricsRecorder _metricsRecorder;

    public TypedDocumentCosmosStore(CosmosClient cosmosClient, IOptions<CosmosDataStoreOptions> options, IMapper mapper,
        IMetricsRecorder metricsRecorder)
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
        _metricsRecorder = metricsRecorder;
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

        var container = GetContainer();
        var result = await container.UpsertItemAsync(document);
        RecordQueryMetrics(result.RequestCharge);
    }

    public async Task<TModel> GetByIdAsync<TModel>(string documentId)
    {
        var container = GetContainer();
        var readResponse = await container.ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(_documentType.Value));
        RecordQueryMetrics(readResponse.RequestCharge);
        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId)
    {
        return GetByIdOrDefaultAsync<TModel>(documentId, _documentType.Value);
    }

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId, string partitionKey)
    {
        var container = GetContainer();

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
        var container = GetContainer();
        var readResponse = await container.ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey));
        RecordQueryMetrics(readResponse.RequestCharge);
        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        return IterateResults(queryFeed, rs => _mapper.Map<TModel>(rs));
    }

    public async Task DeleteDocument(string documentId, string partitionKey)
    {
        var container = GetContainer();
        var result = await container.DeleteItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey));
        RecordQueryMetrics(result.RequestCharge);
    }

    public Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query)
    {
        var queryFeed = GetContainer().GetItemQueryIterator<TModel>(
            queryDefinition: query);

        return IterateResults(queryFeed, item => item);
    }

    public async Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches)
    {
        if (patches.Count() > 10)
        {
            throw new NotSupportedException("Only 10 patches can be applied");
        }

        var container = GetContainer();
        var result = await container.PatchItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patches.ToList().AsReadOnly());

        RecordQueryMetrics(result.RequestCharge);
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
                //TODO extend this to pass a cancellation token in for the fire and forget version
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
