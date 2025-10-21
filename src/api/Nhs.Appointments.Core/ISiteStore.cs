namespace Nhs.Appointments.Core;

public interface ISiteStore
{    
    Task<Site> GetSiteById(string siteId);
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities, bool allowUpdatesToDeletedSites = true);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens, bool allowUpdatesToDeletedSites = true);

    Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber,
        decimal? longitude,
        decimal? latitude, 
        bool allowUpdatesToDeletedSites = true);
    
    Task<OperationResult> UpdateSiteReferenceDetails(string siteId, string odsCode, string icb, string region, bool allowUpdatesToDeletedSites = true);
    
    Task AssignPrefix(string site, int prefix);
    Task<int> GetReferenceNumberGroup(string site);
    Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false);

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type, SiteStatus? siteStatus = null, bool? isDeleted = null, bool allowUpdatesToDeletedSites = true);

    Task<IEnumerable<Site>> GetSitesInRegionAsync(string region);

    Task<OperationResult> UpdateSiteStatusAsync(string siteId, SiteStatus status, bool allowUpdatesToDeletedSites = true);

    Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb);
}
