namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class BookingReferenceDocument : CoreDataCosmosDocument
{
    public int Sequence { get; set; }
}
