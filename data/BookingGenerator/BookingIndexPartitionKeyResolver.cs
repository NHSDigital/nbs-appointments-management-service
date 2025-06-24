using Nhs.Appointments.Persistance.Models;

namespace BookingGenerator;

public class BookingIndexPartitionKeyResolver : IPartitionKeyResolver<BookingIndexDocument>
{
    public string ResolvePartitionKey(BookingIndexDocument document) => document.DocumentType;
}
