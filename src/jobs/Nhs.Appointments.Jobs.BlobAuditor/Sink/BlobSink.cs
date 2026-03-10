using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Blob;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public class BlobSink(
    IAzureBlobStorage azureBlobStorage,
    IItemExclusionProcessor exclusionProcessor
) : ISink<JObject>
{
    public async Task Consume(string source, JObject item)
    {
        //don't process any data without this property
        if (!item.TryGetValue("lastUpdatedOn", out var lastUpdatedOn))
        {
            return;
        }

        var entityChangeTimestamp = lastUpdatedOn.Value<DateTime>();
        var containerName = GetContainerName(source, entityChangeTimestamp);

        item.TryGetValue("docType", out var entityDocType);
        var blobName = GetBlobName(entityDocType?.Value<string>() ?? "unknown", source, item, entityChangeTimestamp);

        var filteredItem = exclusionProcessor.Apply(source, item);

        await using var writer = new JsonTextWriter(new StreamWriter(await azureBlobStorage.GetBlobUploadStream(containerName, blobName)));

        await writer.WriteRawAsync(JsonConvert.SerializeObject(filteredItem, Formatting.Indented));
    }

    private static string GetContainerName(string source, DateTime timeStamp) => $"{timeStamp:yyyyMMdd}-{source.Replace("_", "")}";

    private string GetBlobName(string entityDocType, string source, JObject item, DateTime timeStamp) =>
        Path.Combine(entityDocType, $"{timeStamp:HHmmssfffffff}-{item.ResolveIdentifier(source)}.json");
}
