namespace Nhs.Appointments.Core;

public interface ISiteStore
{    
    Task<Site> GetSiteById(string siteId);
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens);

    Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber,
        decimal latitude, decimal longitude);
    
    Task<OperationResult> UpdateSiteReferenceDetails(string siteId, string odsCode, string icb, string region);
    
    Task AssignPrefix(string site, int prefix);
    Task<IEnumerable<Site>> GetAllSites();

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities);
}
