namespace Nhs.Appointments.Core;

public interface ISiteStore
{
    Task<IEnumerable<SiteWithDistance>> GetSitesByArea(double longitude, double latitude, int searchRadius);
    Task<Site> GetSiteById(string siteId);
    Task<OperationResult> UpdateSiteAttributes(string siteId, IEnumerable<AttributeValue> attributeValues);
}
