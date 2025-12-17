using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Blob;
using Nhs.Appointments.Jobs.BlobAuditor.Extensions;

namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public class BlobSink(
    IAzureBlobStorage azureBlobStorage, 
    TimeProvider timeProvider,
    IItemExclusionProcessor exclusionProcessor
) : IBlobSink<JObject>
{
    public async Task Consume(string source, JObject item)
    {
        var now = timeProvider.GetUtcNow();
        var containerName = GetContainerName(source, now);
        var entityChangeTimestamp = now;
        if (item.TryGetValue("_ts", out var timeStamp))
        {
            entityChangeTimestamp = DateTimeOffset.FromUnixTimeSeconds(timeStamp.Value<int>());
        }

        item.TryGetValue("docType", out var entityDocType);
        var blobName = GetBlobName(entityDocType?.Value<string>() ?? "unknown", source, item, entityChangeTimestamp);

        var filteredItem = exclusionProcessor.Apply(source, item);

        await using var writer = new JsonTextWriter(new StreamWriter(await azureBlobStorage.GetBlobUploadStream(containerName, blobName)));

        await writer.WriteRawAsync(JsonConvert.SerializeObject(filteredItem, Formatting.Indented));
    }

    private static string GetContainerName(string source, DateTimeOffset timeStamp) => $"{timeStamp:yyyyMMdd}-{source.Replace("_", "")}";

    private string GetBlobName(string entityDocType, string source, JObject item, DateTimeOffset timeStamp) =>
        Path.Combine(
            $"{timeStamp:yyyyMMdd}", 
            entityDocType,
            $"{timeStamp:yyyyMMddHHmmssfff}-{item.ResolveIdentifier(source)}.json");
}
