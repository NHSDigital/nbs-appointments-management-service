namespace Nhs.Appointments.Core;

public interface ISiteStore
{
    Task<Site> GetSiteById(string siteId);
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens);

    Task<OperationResult> UpdateSiteDetails(string siteId, string name, string address, string phoneNumber,
        decimal? longitude,
        decimal? latitude);

    Task<OperationResult> UpdateSiteReferenceDetails(string siteId, string odsCode, string icb, string region);

    Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false);

    Task AssignPrefix(string site, int prefix);
    Task<int> GetReferenceNumberGroup(string site);

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type,
        SiteStatus? siteStatus = null, bool? isDeleted = null);

    Task<IEnumerable<Site>> GetSitesInRegionAsync(string region);

    Task<OperationResult> UpdateSiteStatusAsync(string siteId, SiteStatus status);

    Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb);
    Task<OperationResult> ToggleSiteSoftDeletionAsync(string siteId);
}
