using Newtonsoft.Json.Linq;

namespace Nhs.Appointments.Jobs.BlobAuditor.Extensions;

public static class JObjectExtensions
{
    public static string ResolveIdentifier(this JObject entity, string source) => source switch
    {
        DataType.AuditData => entity.Value<string>("id"),
        DataType.BookingData => $"{entity.Value<string>("id")}-{entity.Value<string>("site")}",
        DataType.CoreData or DataType.IndexData => $"{entity.Value<string>("id")}-{entity.Value<string>("docType")}",
        _ => throw new NotImplementedException($"{source} is not recognized"),
    } ?? throw new InvalidOperationException($"entity is not valid for {source}");
}
