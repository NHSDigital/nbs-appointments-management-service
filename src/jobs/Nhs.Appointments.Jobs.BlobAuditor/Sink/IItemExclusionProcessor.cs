using Newtonsoft.Json.Linq;

namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public interface IItemExclusionProcessor
{
    JObject Apply(string source, JObject item);
}
