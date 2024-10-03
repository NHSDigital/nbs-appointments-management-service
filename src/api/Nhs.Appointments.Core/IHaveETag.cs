using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public interface IHaveETag
{
    string ETag { get; set; }
}

public record DomainRoot : IHaveETag
{
    [JsonProperty("_etag")]
    public string ETag { get;set; }
}
