using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class DailyAvailabilityPartitionKeyResolver : IPartitionKeyResolver<DailyAvailabilityDocument>
{
    public string ResolvePartitionKey(DailyAvailabilityDocument document) => document.Site;
}
