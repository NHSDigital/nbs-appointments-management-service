using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class ReferenceGroupPartitionKeyResolver : IPartitionKeyResolver<ReferenceGroupDocument>
{
    public string ResolvePartitionKey(ReferenceGroupDocument document) => document.DocumentType;
}
