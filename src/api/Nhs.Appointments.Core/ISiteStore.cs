namespace Nhs.Appointments.Core;

public interface ISiteStore
{    
    Task<Site> GetSiteById(string siteId);
    Task<OperationResult> UpdateSiteAttributes(string siteId, string scope, IEnumerable<AttributeValue> attributeValues);

    Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber,
        string latitude, string longitude);
    Task AssignPrefix(string site, int prefix);
    Task<int> GetReferenceNumberGroup(string site);
    Task<IEnumerable<Site>> GetAllSites();
}
