using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using System.Linq.Expressions;

namespace Nhs.Appointments.Persistance;

public interface ITypedDocumentCosmosStore<TDocument> 
{
    Task<TModel> GetByIdAsync<TModel>(string documentId) where TModel : IHaveETag;    
    Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate) where TModel : IHaveETag;
    Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query) where TModel : IHaveETag;
    Task<TModel> GetDocument<TModel>(string documentId) where TModel : IHaveETag;
    Task<TModel> GetDocument<TModel>(string documentId, string partitionKey) where TModel : IHaveETag;
    Task DeleteDocument(string documentId, string partitionKey);
    Task WriteAsync(TDocument document, Action<TDocument>? OnConcurrencyError = null);
    TDocument NewDocument();
    TDocument ConvertToDocument<TModel>(TModel model) where TModel : IHaveETag;
    Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches);
    string GetDocumentType();
}