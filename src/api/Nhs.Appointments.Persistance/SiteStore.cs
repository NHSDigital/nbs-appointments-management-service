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

    public async Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        var siteDocumentPatch = PatchOperation.Add("/accessibilities", accessibilities.Where(a => !string.IsNullOrEmpty(a.Value)));
        await cosmosStore.PatchDocument(documentType, siteId, siteDocumentPatch);
        return new OperationResult(true);
    }
    public async Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        var siteDocumentPatch = PatchOperation.Set("/informationForCitizens", informationForCitizens);
        await cosmosStore.PatchDocument(documentType, siteId, siteDocumentPatch);
        return new OperationResult(true);
    }
    
    public async Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber, decimal latitude, decimal longitude)
    {
        decimal[] coords = [latitude, longitude];
        
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        PatchOperation[] detailsPatchOperations =
        [
            PatchOperation.Replace("/name", name),
            PatchOperation.Replace("/address", address),
            PatchOperation.Replace("/phoneNumber", phoneNumber),
            PatchOperation.Replace("/location/coordinates", coords),
        ];

        await cosmosStore.PatchDocument(documentType, siteId, detailsPatchOperations);
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
