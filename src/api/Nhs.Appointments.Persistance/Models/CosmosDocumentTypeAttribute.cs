namespace Nhs.Appointments.Persistance.Models;

[AttributeUsage(AttributeTargets.Class)]
public class CosmosDocumentTypeAttribute : Attribute
{
    public string Value { get; init; }

    public CosmosDocumentTypeAttribute(string value)
    {
        Value = value;
    }
}
