using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nhs.Appointments.Persistance;
using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class TypeFileStore<TDocument> : ITypedDocumentCosmosStore<TDocument>
    where TDocument : TypedCosmosDocument
{
    public TypeFileStore(IMapper mapper)
    {
        _documentType = new Lazy<string>(GetDocumentType);
        _mapper = mapper;
        var cosmosDocumentAttribute = typeof(TDocument).GetCustomAttribute<CosmosDocumentAttribute>(true);
        if (cosmosDocumentAttribute == null)
        {
            throw new NotSupportedException("Document type must have a CosmosDocument attribute");
        }

        _containerName = $"output/{cosmosDocumentAttribute.ContainerName}";
    }
    
    private readonly Lazy<string> _documentType;
    internal readonly string _containerName;
    private readonly IMapper _mapper;
    
    public async Task<TModel> GetByIdAsync<TModel>(string documentId) => await GetDocument<TModel>(documentId);

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId, string partitionKey)
    {
        if (File.Exists($"{partitionKey}/{documentId}.json"))
        {
            var document = await GetDocument<TDocument>(documentId, partitionKey);
            return _mapper.Map<TModel>(document);
        }

        return default;
    }

    public async Task<TModel> GetByIdOrDefaultAsync<TModel>(string documentId) =>
        await GetByIdOrDefaultAsync<TModel>(documentId, _containerName);

    public async Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate)
    {
        var files = Directory.GetFiles(_containerName);

        var models = await Task.WhenAll(files.Select(TryRead));
        return models.AsEnumerable().Where(predicate.Compile()).Select(_mapper.Map<TModel>);
    }

    private async Task<TDocument> TryRead(string fileName)
    {
        try
        {
            return JsonConvert.DeserializeObject<TDocument>((await File.ReadAllTextAsync(fileName)));
        }
        catch
        {
            return default;
        }
    }

    public Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query) => throw new NotImplementedException();

    public async Task<TModel> GetDocument<TModel>(string documentId) => await GetDocument<TModel>(_containerName, documentId);

    public async Task<TModel> GetDocument<TModel>(string documentId, string partitionKey)
    {
        var text = await File.ReadAllTextAsync($"{partitionKey}/{documentId}.json");

        var document = JsonConvert.DeserializeObject<TDocument>(text);

        return _mapper.Map<TModel>(document);
    }

    public Task DeleteDocument(string documentId, string partitionKey) => throw new NotImplementedException();

    public async Task WriteAsync(TDocument document)
    {
        await WriteToFile(document);
    }

    private async Task WriteToFile(TDocument document)
    {
        if (!Directory.Exists(_containerName))
        {
            Directory.CreateDirectory(_containerName);
        }

        File.WriteAllText($"{_containerName}/{document.Id}.json", JsonConvert.SerializeObject(document, Formatting.Indented, 
            new JsonSerializerSettings()
            {
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            }));
    }

    public TDocument NewDocument()
    {
        throw new NotImplementedException();
    }

    public TDocument ConvertToDocument<TModel>(TModel model)
    {
        var document = _mapper.Map<TDocument>(model);
        document.DocumentType = _documentType.Value;
        return document;
    }

    public Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches) => throw new NotImplementedException();

    public string GetDocumentType()
    {
        return typeof(TDocument).GetCustomAttribute<CosmosDocumentTypeAttribute>()!.Value;
    }
    
    private async Task<IEnumerable<TOutput>> IterateResults<TSource, TOutput>(FeedIterator<TSource> queryFeed,
        Func<TSource, TOutput> map)
    {
        var results = new List<TOutput>();
        using (queryFeed)
        {
            while (queryFeed.HasMoreResults)
            {
                var resultSet = await queryFeed.ReadNextAsync();
                results.AddRange(resultSet.Select(map));
            }
        }
        
        return results;
    }
}
