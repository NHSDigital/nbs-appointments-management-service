using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal interface IPartitionKeyResolver<TDocument> where TDocument : TypedCosmosDocument
{
    string ResolvePartitionKey(TDocument document);
}
