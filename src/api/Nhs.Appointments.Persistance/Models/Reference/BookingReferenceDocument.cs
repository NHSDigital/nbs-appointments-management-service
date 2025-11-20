namespace Nhs.Appointments.Persistance.Models.Reference;

[CosmosDocumentType("system")]
public class BookingReferenceDocument : CoreDataCosmosDocument
{
    public int Sequence { get; set; }
    public PartitionKeyMultiplier[] PartitionKeyMultipliers { get; set; }
}

public class PartitionKeyMultiplier
{
    public string PartitionKey { get; set; }
    public int SequenceMultiplier { get; set; }
}
