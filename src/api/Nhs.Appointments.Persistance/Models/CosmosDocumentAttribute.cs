namespace Nhs.Appointments.Persistance.Models;

[AttributeUsage(AttributeTargets.Class)]
public class CosmosDocumentAttribute : Attribute
{
    public string ContainerName { get; init; }
    public string PartitionKey { get; init; }

    public CosmosDocumentAttribute(string containerName, string partitionKey)
    {
        ContainerName = containerName;
        PartitionKey = partitionKey;
    }
}
