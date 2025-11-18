namespace Nhs.Appointments.Persistance.Models.Reference;

[CosmosDocumentType("system")]
public class BookingReferenceDocument : CoreDataCosmosDocument
{
    public int Sequence { get; set; }
}
