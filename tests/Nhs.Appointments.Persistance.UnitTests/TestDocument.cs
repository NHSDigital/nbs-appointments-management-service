using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance.UnitTests;

[CosmosDocument("test-container", "test")]
[CosmosDocumentType("test_doc")]
public class TestDocument : LastUpdatedByCosmosDocument
{
    public string Name { get; set; }
}
