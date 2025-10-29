namespace Nhs.Appointments.Persistance.Models.Reference;

[CosmosDocumentType("reference_group")]
public class ReferenceGroupDocument : CoreDataCosmosDocument
{
    public ReferenceGroup[] Groups { get; set; }
}

public class ReferenceGroup
{
    public int Prefix { get; set; }
    public int SiteCount { get; set; }
    public int Sequence { get; set; }
}
