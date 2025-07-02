using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class AvailabilityCreateEventPartitionKeyResolver : IPartitionKeyResolver<AvailabilityCreatedEventDocument>
{
    public string ResolvePartitionKey(AvailabilityCreatedEventDocument document) => document.Site;
}
