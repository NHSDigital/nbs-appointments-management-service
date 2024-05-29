using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace Nhs.Appointments.Persistance;

public class TypedDocumentCosmosStore<TDocument> : ITypedDocumentCosmosStore<TDocument> where TDocument : TypedCosmosDocument, new()
{
    private readonly CosmosClient _cosmosClient;
    private readonly IMapper _mapper;
    private readonly string _databaseName;
    private readonly string _containerName;
    private readonly Lazy<string> _documentType;

    public TypedDocumentCosmosStore(CosmosClient cosmosClient, IOptions<CosmosDataStoreOptions> options, IMapper mapper)
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

    public Task WriteAsync(TDocument document)
    {
        if (document.DocumentType != _documentType.Value)
            throw new InvalidOperationException("Document type does not match the supported type for this writer");
        var container = GetContainer();
        return container.UpsertItemAsync(document);
    }

    public async Task<TModel> GetByIdAsync<TModel>(string documentId)
    {
        var container = GetContainer();
        var readResponse = await container.ReadItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(_documentType.Value));
        return _mapper.Map<TModel>(readResponse.Resource);
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
        return _mapper.Map<TModel>(readResponse.Resource);
    }

    public async Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate)
    {
        var queryFeed = GetContainer().GetItemLinqQueryable<TDocument>().Where(predicate).ToFeedIterator();
        var results = new List<TModel>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet.Select(rs => _mapper.Map<TModel>(rs)));
            }
        }
        return results;
    }

    public async Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches)
    {
        if (patches.Count() > 10)
            throw new NotSupportedException("Only 10 patches can be applied");

        var container = GetContainer();
        var result = await container.PatchItemAsync<TDocument>(
            id: documentId,
            partitionKey: new PartitionKey(partitionKey),
            patchOperations: patches.ToList().AsReadOnly());

        return result.Resource;
    }    

    public string GetDocumentType() => typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;

    protected Container GetContainer() => _cosmosClient.GetContainer(_databaseName, _containerName);
}
