using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Cosmos;

namespace Nhs.Appointments.Jobs.BlobAuditor.Extensions;

public static class JObjectExtensions
{
    public static string ResolveIdentifier(this JObject entity, string source) => source switch
    {
        ContainerName.AuditData => entity.Value<string>("id"),
        ContainerName.BookingData => $"{entity.Value<string>("id")}-{entity.Value<string>("site")}",
        ContainerName.AggregatedData => $"{entity.Value<string>("id")}-{entity.Value<string>("date")}",
        ContainerName.CoreData or ContainerName.IndexData => $"{entity.Value<string>("id")}-{entity.Value<string>("docType")}",
        _ => throw new NotImplementedException($"{source} is not recognized"),
    } ?? throw new InvalidOperationException($"entity is not valid for {source}");
}
