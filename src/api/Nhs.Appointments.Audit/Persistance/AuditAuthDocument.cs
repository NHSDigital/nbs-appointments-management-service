using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("auth")]
public class AuditAuthDocument : AuditDataCosmosDocument
{
    [JsonProperty("actionType")] 
    public AuditAuthActionType ActionType { get; set; }
}

public enum AuditAuthActionType
{
    Undefined = 0,
    Login = 1
}
