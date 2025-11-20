using CosmosAuditor.Blob;
using CosmosAuditor.Containers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosAuditor.AuditSinks;

public class BlobAuditSink(IAzureBlobStorage azureBlobStorage) : IAuditSink
{
    public string Name => "BlobSink";

    public async Task Consume(ContainerConfig config, JObject item)
    {
        var containerName = $"{DateTime.UtcNow:yyyyMMdd}-{config.Name.Replace("_", "") }-audit";
        var entityChangeDateTime = DateTimeOffset.FromUnixTimeSeconds(item.GetValue("_ts").Value<int>());
        var entityDocType = item.GetValue("docType").Value<string>();
        var blobName = Path.Combine($"{entityChangeDateTime:yyyyMMdd}", entityDocType, $"{entityChangeDateTime:yyyyMMddHHmmssfff}-{config.IdResolver(item)}.json");
        
        await using var writer = new JsonTextWriter(new StreamWriter(await azureBlobStorage.GetBlobUploadStream(containerName, blobName)));
        await writer.WriteRawAsync(JsonConvert.SerializeObject(item));
    }
}
