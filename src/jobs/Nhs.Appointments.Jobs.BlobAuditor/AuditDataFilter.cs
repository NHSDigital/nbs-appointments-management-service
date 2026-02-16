using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor;

public class AuditDataFilter : IDataFilter<JObject>
{
    public bool IsValidItem(JObject item) => true;
}
