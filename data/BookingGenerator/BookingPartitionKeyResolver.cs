using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class BookingPartitionKeyResolver : IPartitionKeyResolver<BookingDocument>
{
    public string ResolvePartitionKey(BookingDocument document) => document.Site;
}
