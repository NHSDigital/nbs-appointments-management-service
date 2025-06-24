using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public interface IPartitionKeyResolver<TDocument> where TDocument : TypedCosmosDocument
{
    string ResolvePartitionKey(TDocument document);
}
