using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class BookingPartitionKeyResolver : IPartitionKeyResolver<BookingDocument>
{
    public string ResolvePartitionKey(BookingDocument document) => document.Site;
}
