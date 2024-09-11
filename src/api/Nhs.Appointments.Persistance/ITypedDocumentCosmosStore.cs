using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;

namespace Nhs.Appointments.Persistance;

public interface ITypedDocumentCosmosStore<TDocument> 
{
    Task<TModel> GetByIdAsync<TModel>(string documentId);    
    Task<IEnumerable<TModel>> RunQueryAsync<TModel>(Expression<Func<TDocument, bool>> predicate);
    Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query);
    Task<TModel> GetDocument<TModel>(string documentId);
    Task<TModel> GetDocument<TModel>(string documentId, string partitionKey);
    Task WriteAsync(TDocument document);
    TDocument NewDocument();
    TDocument ConvertToDocument<TModel>(TModel model);
    Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches);
    string GetDocumentType();
}