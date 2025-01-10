using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocument("audit_data", "userId")]
public class AuditDataCosmosDocument : TypedCosmosDocument
{

}
