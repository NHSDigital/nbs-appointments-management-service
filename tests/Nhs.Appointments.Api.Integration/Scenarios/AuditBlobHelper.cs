using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nhs.Appointments.Api.Integration.Scenarios;

public static class AuditBlobHelper
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan PollingTimeout = TimeSpan.FromSeconds(20);

    public static async Task<string> PollForAuditLogAsync(BlobServiceClient blobServiceClient, string containerSource, string fileName)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < PollingTimeout)
        {
            var content = await ReadAuditJsonByNameAsync(blobServiceClient, containerSource, fileName);

            if (content != null)
            {
                return content;
            }

            await Task.Delay(PollingInterval);
        }

        return null;
    }

    public static string GetBlobName(
        string documentType, 
        DateTimeOffset timeStamp,
        string identifier

    ) => Path.Combine(
        $"{timeStamp:yyyyMMdd}",
        documentType,
        $"{timeStamp:yyyyMMddHHmmssfff}-{identifier}.json"
    );

    private static async Task<string> ReadAuditJsonByNameAsync(BlobServiceClient blobServiceClient, string containerSource, string fileName)
    {
        var containerName = $"{DateTime.UtcNow:yyyyMMdd}-{containerSource.Replace("_", "")}";
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync())
        {
            return null;
        }

        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }
}

public class TimeOnlyConverter : JsonConverter<TimeOnly>
{
    private const string Format = "HH:mm";

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
        => writer.WriteValue(value.ToString(Format));

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
        => TimeOnly.Parse(((string)reader.Value)!);
}
