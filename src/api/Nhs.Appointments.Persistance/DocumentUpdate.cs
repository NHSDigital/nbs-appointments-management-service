using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nhs.Appointments.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace Nhs.Appointments.Persistance;

public class DocumentUpdate<TModel, TDocument> : IDocumentUpdate<TModel>
{
    private readonly string _partitionKey;
    private readonly string _documentId;
    private readonly ITypedDocumentCosmosStore<TDocument> _store;
    private readonly List<PatchOperation> _patches = new List<PatchOperation>();

    public DocumentUpdate(ITypedDocumentCosmosStore<TDocument> store, string partitionKey, string documentId)
    {
        _store = store;
        _partitionKey = partitionKey;
        _documentId = documentId;
    }

    public IDocumentUpdate<TModel> UpdateProperty<TProp>(Expression<Func<TModel, TProp>> prop, TProp val)
    {
        var propertyExpression = prop.Body as MemberExpression;
        if (propertyExpression == null)
            throw new NotSupportedException("Only property expression are supported in document update");

        var propertyName = propertyExpression.Member.Name;
        var jsonAttribute = propertyExpression.Member.GetCustomAttribute<JsonPropertyAttribute>();
        if (jsonAttribute != null)
        {
            propertyName = jsonAttribute.PropertyName;
        }

        _patches.Add(PatchOperation.Replace($"/{propertyName}", val));

        return this;
    }

    public Task ApplyAsync()
    {
        if (_patches.Count > 10)
            throw new NotSupportedException("Only 10 patches can be applied");

        return _store.PatchDocument(_partitionKey, _documentId, _patches.ToArray());
    }
}
