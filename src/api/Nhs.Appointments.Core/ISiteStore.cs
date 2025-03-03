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
    Task<int> GetReferenceNumberGroup(string site);
    Task<IEnumerable<Site>> GetAllSites();
}
