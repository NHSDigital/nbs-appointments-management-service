namespace Nhs.Appointments.Core;

public interface IReferenceNumberProvider
{
    Task<string> GetReferenceNumber();
}

public class ReferenceNumberProvider(
    IReferenceNumberDocumentStore referenceNumberDocumentStore,
    TimeProvider timeProvider)
    : IReferenceNumberProvider
{
    public async Task<string> GetReferenceNumber()
    {
        var sequenceNumber = await referenceNumberDocumentStore.GetNextSequenceNumber();

        if (sequenceNumber is < 1000000 or > 999999999)
        {
            throw new NotSupportedException($"Booking reference generation is not supported for the provided sequence number: {sequenceNumber}");
        }
        
        var now = timeProvider.GetUtcNow();
        var rng = now.Day + now.Second;
        var sequenceAsString = $"{sequenceNumber:000000000}";

        return $"{sequenceAsString[..3]}-{rng:00}-{sequenceAsString[^6..]}";
    }
}

public interface IReferenceNumberDocumentStore
{
    Task<int> GetNextSequenceNumber();
}
