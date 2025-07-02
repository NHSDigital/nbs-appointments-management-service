using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class ReferenceGroupPartitionKeyResolver : IPartitionKeyResolver<ReferenceGroupDocument>
{
    public string ResolvePartitionKey(ReferenceGroupDocument document) => document.DocumentType;
}
