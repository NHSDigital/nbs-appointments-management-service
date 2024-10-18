using System.Reflection.Metadata;

namespace Nhs.Appointments.Core;

public interface IReferenceNumberProvider
{
    Task<string> GetReferenceNumber(string siteId);
}

public class ReferenceNumberProvider : IReferenceNumberProvider
{
    private readonly ISiteStore _siteStore;
    private readonly IReferenceNumberDocumentStore _referenceNumberDocumentStore;
    private readonly TimeProvider _timeProvider;
    public ReferenceNumberProvider(
        ISiteStore siteStore,
        IReferenceNumberDocumentStore referenceNumberDocumentStore,
        TimeProvider timeProvider)
    {
        _siteStore = siteStore;
        _referenceNumberDocumentStore = referenceNumberDocumentStore;
        _timeProvider = timeProvider;
    }
    public async Task<string> GetReferenceNumber(string siteId)
    {        
        var referenceGroup = await _siteStore.GetReferenceNumberGroup(siteId);
        if (referenceGroup == 0)
        {
            referenceGroup = await _referenceNumberDocumentStore.AssignReferenceGroup();
            await _siteStore.AssignPrefix(siteId, referenceGroup);
        }

        var sequence = await _referenceNumberDocumentStore.GetNextSequenceNumber(referenceGroup);
        var now = _timeProvider.GetUtcNow();
        var rng = now.Day + now.Second;

        return $"{referenceGroup:00}-{rng:00}-{sequence:000000}";
    }
}

public interface IReferenceNumberDocumentStore
{
    Task<int> AssignReferenceGroup();
    Task<int> GetNextSequenceNumber(int prefix);
}
    