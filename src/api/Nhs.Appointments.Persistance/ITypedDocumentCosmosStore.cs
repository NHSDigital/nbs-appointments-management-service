using Microsoft.Azure.Cosmos;
using System.Linq.Expressions;

namespace Nhs.Appointments.Persistance;

public interface ITypedDocumentCosmosStore<TDocument> 
{
    Task<TDocument> GetByIdAsync(string documentId);
    Task<TDocument> GetByIdOrDefaultAsync(string documentId, string partitionKey);
    Task<TDocument> GetByIdOrDefaultAsync(string documentId);
    Task<IEnumerable<TDocument>> RunQueryAsync(Expression<Func<TDocument, bool>> predicate);
    Task<IEnumerable<TDocument>> RunSqlQueryAsync(QueryDefinition query);
    Task<TDocument> GetDocument(string documentId);
    Task<TDocument> GetDocument(string documentId, string partitionKey);
    Task DeleteDocument(string documentId, string partitionKey);
    Task WriteAsync(TDocument document);
    TDocument NewDocument();
    Task<TDocument> PatchDocument(string partitionKey, string documentId, params PatchOperation[] patches);
    string GetDocumentType();
    Task<IEnumerable<TModel>> RunSqlQueryAsync<TModel>(QueryDefinition query);
}
