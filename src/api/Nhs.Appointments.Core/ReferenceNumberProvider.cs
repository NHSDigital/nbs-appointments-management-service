namespace Nhs.Appointments.Core;

public interface IReferenceNumberProvider
{
    Task<string> GetReferenceNumber();
}

public class ReferenceNumberProvider(
    IBookingReferenceDocumentStore bookingReferenceDocumentStore,
    TimeProvider timeProvider)
    : IReferenceNumberProvider
{
    /// <summary>
    /// Generate the booking reference number in a way that is future-proof, for the very long term (100 years)
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetReferenceNumber()
    {
        var sequenceNumber = await bookingReferenceDocumentStore.GetNextSequenceNumber();

        //the max sequence number part we are going to use is 100 million, so reset to 0 after
        var overflowedSequence = sequenceNumber % 100000000;
        var overflowedSequenceAsString = $"{overflowedSequence:00000000}";
        
        var now = timeProvider.GetUtcNow();
        
        var yearAsString = $"{now.Year:0000}";
        
        //divide total days into 4, result is always < 100, so can use 2 digits
        //also makes it look more random than using just the week day
        var dayPartition = (now.DayOfYear / 4) + 1;
        var dayPartitionAsString = $"{dayPartition:00}";

        return $"{dayPartitionAsString}{yearAsString[^2..]}-{overflowedSequenceAsString[..4]}-{overflowedSequenceAsString[^4..]}";
    }
}

public interface IBookingReferenceDocumentStore
{
    Task<int> GetNextSequenceNumber();
}
