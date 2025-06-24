using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class DailyAvailabilityPartitionKeyResolver : IPartitionKeyResolver<DailyAvailabilityDocument>
{
    public string ResolvePartitionKey(DailyAvailabilityDocument document) => document.Site;
}
