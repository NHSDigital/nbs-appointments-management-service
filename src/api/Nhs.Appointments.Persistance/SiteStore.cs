using System.Net;
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

    public async Task<OperationResult> UpdateSiteReferenceDetails(string siteId, string odsCode, string icb, string region)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument == null)
        {
            return new OperationResult(false, "The specified site was not found.");
        }
        var documentType = cosmosStore.GetDocumentType();
        PatchOperation[] detailsPatchOperations =
        [
            PatchOperation.Replace("/odsCode", odsCode),
            PatchOperation.Replace("/integratedCareBoard", icb),
            PatchOperation.Replace("/region", region)
        ];

        await cosmosStore.PatchDocument(documentType, siteId, detailsPatchOperations);
        return new OperationResult(true);
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

    public async Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber,
        decimal? longitude, decimal? latitude)
    {
        decimal?[] coords = (longitude != null) & (latitude != null) ? [longitude, latitude] : [];
        
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
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    public async Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument is null)
        {
            var site = new SiteDocument
            {
                Id = siteId,
                OdsCode = odsCode,
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                DocumentType = "site",
                Accessibilities = accessibilities.ToArray(),
                InformationForCitizens = string.Empty,
                IntegratedCareBoard = icb,
                Location = location,
                Region = region,
                Type = type
            };
            var document = cosmosStore.ConvertToDocument(site);
            await cosmosStore.WriteAsync(document);

            return new OperationResult(true);
        }
        else
        {
            var updateSite = UpdateSiteDetails(siteId, name, address, phoneNumber, (decimal)location.Coordinates[0], (decimal)location.Coordinates[1]);
            var updateAccessiblities = UpdateAccessibilities(siteId, accessibilities);

            await Task.WhenAll(updateSite, updateAccessiblities);

            return new OperationResult(true);
        }
    }

    public Task<IEnumerable<Site>> GetSitesInRegionAsync(string region)
    {
        return cosmosStore.RunQueryAsync<Site>(sd => sd.DocumentType == "site" && sd.Region == region);
    }

    public async Task<OperationResult> UpdateSiteStatusAsync(string siteId, SiteStatus status)
    {
        var originalDocument = await GetOrDefault(siteId);
        if (originalDocument is null)
        {
            return new OperationResult(false, $"The specified site: {siteId} was not found.");
        }

        var documentType = cosmosStore.GetDocumentType();

        var patchOperation = originalDocument.status is null
            ? PatchOperation.Add("/status", status)
            : PatchOperation.Replace("/status", status);

        PatchOperation[] patchOperations = [ patchOperation ];

        await cosmosStore.PatchDocument(documentType, siteId, patchOperations);

        return new OperationResult(true);
    }
}
