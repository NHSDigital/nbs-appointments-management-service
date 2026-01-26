using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor;

public class AuditFeedEventMapper : IFeedEventMapper<JObject, JObject>
{
    public IEnumerable<JObject> MapToEvents(IEnumerable<JObject> feedItems) => feedItems;
}
