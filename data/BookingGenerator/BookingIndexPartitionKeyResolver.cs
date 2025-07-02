using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

internal class BookingIndexPartitionKeyResolver : IPartitionKeyResolver<BookingIndexDocument>
{
    public string ResolvePartitionKey(BookingIndexDocument document) => document.DocumentType;
}
