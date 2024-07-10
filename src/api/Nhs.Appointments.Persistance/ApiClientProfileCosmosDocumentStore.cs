using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class ApiClientProfileCosmosDocumentStore : IApiClientProfileStore
{
    private readonly ITypedDocumentCosmosStore<ApiClientProfileDocument> _innerStore;

    public ApiClientProfileCosmosDocumentStore(ITypedDocumentCosmosStore<ApiClientProfileDocument> innerStore)
    {
        _innerStore = innerStore;
    }

    public Task<ApiClientProfile> GetAsync(string id) => _innerStore.GetByIdAsync<ApiClientProfile>(id);
    
}