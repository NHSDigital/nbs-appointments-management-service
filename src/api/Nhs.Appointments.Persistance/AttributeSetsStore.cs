using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AttributeSetsStore(ITypedDocumentCosmosStore<AttributeSetsDocument> cosmosStore) : IAttributeSetsStore
{
    private const string AttributeSetsDocumentId = "attribute_sets";
    public Task<AttributeSets> GetAttributeSets()
    {
        return cosmosStore.GetDocument<AttributeSets>(AttributeSetsDocumentId);
    }
}