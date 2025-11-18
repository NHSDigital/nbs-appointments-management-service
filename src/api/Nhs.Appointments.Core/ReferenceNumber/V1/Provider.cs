using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.ReferenceNumber.V1;

public interface IReferenceNumberProvider
{
    [Obsolete("Deprecated in favor of ReferenceNumber.V2.IProvider.GetReferenceNumber")]
    Task<string> GetReferenceNumber(string siteId);
}

[Obsolete("Deprecated in favor of ReferenceNumber.V2.Provider")]
public class ReferenceNumberProvider(
    ISiteStore siteStore,
    IReferenceNumberDocumentStore referenceNumberDocumentStore,
    TimeProvider timeProvider)
    : IReferenceNumberProvider
{
    [Obsolete("Deprecated in favor of ReferenceNumber.V2.Provider.GetReferenceNumber")]
    public async Task<string> GetReferenceNumber(string siteId)
    {        
        var referenceGroup = await siteStore.GetReferenceNumberGroup(siteId);
        if (referenceGroup == 0)
        {
            referenceGroup = await referenceNumberDocumentStore.AssignReferenceGroup();
            await siteStore.AssignPrefix(siteId, referenceGroup);
        }

        var sequence = await referenceNumberDocumentStore.GetNextSequenceNumber(referenceGroup);
        var now = timeProvider.GetUtcNow();
        var rng = now.Day + now.Second;

        return $"{referenceGroup:00}-{rng:00}-{sequence:000000}";
    }
}

[Obsolete("Deprecated in favor of ReferenceNumber.V2.IBookingReferenceDocumentStore")]
public interface IReferenceNumberDocumentStore
{
    Task<int> AssignReferenceGroup();
    Task<int> GetNextSequenceNumber(int prefix);
}
