using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class AvailabilityCreateEventPartitionKeyResolver : IPartitionKeyResolver<AvailabilityCreatedEventDocument>
{
    public string ResolvePartitionKey(AvailabilityCreatedEventDocument document) => document.Site;
}
