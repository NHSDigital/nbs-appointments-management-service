namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class ReferenceGroupDocument : CoreDataCosmosDocument
{
    public int Sequence { get; set; }
}
