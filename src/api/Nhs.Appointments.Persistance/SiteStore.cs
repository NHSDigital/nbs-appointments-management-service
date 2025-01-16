using AutoMapper;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class SiteStore(ITypedDocumentCosmosStore<SiteDocument> cosmosStore) : ISiteStore
{
    
    public Task<Site> GetSiteById(string siteId)
    {
        return GetOrDefault(siteId);
    }

    public Task<IEnumerable<Site>> GetAllSites()
    {
        return cosmosStore.RunQueryAsync<Site>(sd => sd.DocumentType == "site");
    }

    public async Task<int> GetReferenceNumberGroup(string site)
    {
        var siteDocument = await cosmosStore.GetDocument<SiteDocument>(site);
        return siteDocument.ReferenceNumberGroup;
    }

    public Task AssignPrefix(string site, int prefix)
    {
        var updatePrefix = PatchOperation.Set("/referenceNumberGroup", prefix);
        var partitionKey = cosmosStore.GetDocumentType();
        return cosmosStore.PatchDocument(partitionKey, site, updatePrefix);
    }

    public async Task<OperationResult> UpdateSiteAttributes(string siteId, string scope, IEnumerable<AttributeValue> attributeValues)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        var originalAttributes = originalDocument.AttributeValues;
        var newAttributes = scope == "*"
            ? attributeValues
            : originalAttributes
                .Where(a => !a.Id.Contains(scope))
                .Concat(attributeValues);
        var siteDocumentPatch = PatchOperation.Add("/attributeValues", newAttributes.Where(a => !string.IsNullOrEmpty(a.Value)));
        await cosmosStore.PatchDocument(documentType, siteId, siteDocumentPatch);
        return new OperationResult(true);
    }
    
    public async Task<OperationResult> UpdateSiteDetails(string siteId, string name)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        var updateStatusPatch = PatchOperation.Replace("/name", name);

        await cosmosStore.PatchDocument(documentType, siteId, updateStatusPatch);
        return new OperationResult(true);
    }
    
    private async Task<Site> GetOrDefault(string siteId)
    {
        try
        {
            return await cosmosStore.GetDocument<Site>(siteId);
        }
        catch (CosmosException ex) when ( ex.StatusCode == System.Net.HttpStatusCode.NotFound )
        {
            return default;
        }
    }
    
}
