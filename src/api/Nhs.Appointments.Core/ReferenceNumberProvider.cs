using System.Reflection.Metadata;

namespace Nhs.Appointments.Core;

public interface IReferenceNumberProvider
{
    Task<string> GetReferenceNumber(string siteId);
}

public class ReferenceNumberProvider : IReferenceNumberProvider
{
    private readonly IReferenceNumberDocumentStore _referenceNumberDocumentStore;
    private readonly TimeProvider _timeProvider;
    public ReferenceNumberProvider(
        IReferenceNumberDocumentStore referenceNumberDocumentStore,
        TimeProvider timeProvider)
    { 
        _referenceNumberDocumentStore = referenceNumberDocumentStore;
        _timeProvider = timeProvider;
    }
    public async Task<string> GetReferenceNumber(string siteId)
    {        
        var sequence = await _referenceNumberDocumentStore.GetNextSequenceNumber();
        var now = _timeProvider.GetUtcNow();
        var rng = now.Day + now.Second;
        var sequenceAsString = $"{sequence:000000000}";

        return $"{sequenceAsString[0..3]}-{rng:00}-{sequenceAsString[^6..^0]}";
    }
}

public interface IReferenceNumberDocumentStore
{    
    Task<int> GetNextSequenceNumber();
}
