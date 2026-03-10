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

    public static async Task<string> PollForAuditLogAsync(BlobServiceClient blobServiceClient, DateTime timeStamp, string containerSource, string fileName)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < PollingTimeout)
        {
            var content = await ReadAuditJsonByNameAsync(blobServiceClient, timeStamp, containerSource, fileName);

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
        DateTime timeStamp,
        string identifier

    ) => Path.Combine(documentType, $"{timeStamp:HHmmssfffffff}-{identifier}.json"
    );

    private static async Task<string> ReadAuditJsonByNameAsync(BlobServiceClient blobServiceClient, DateTime timeStamp, string containerSource, string fileName)
    {
        var containerName = $"{timeStamp:yyyyMMdd}-{containerSource.Replace("_", "")}";
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

        string contentStringResponse;

        try
        {
            contentStringResponse = response?.Value?.Content?.ToString();
        }
        catch (ArgumentNullException _)
        {
            //catch _bytes not yet populated in BinaryData and return null
            return null;
        }

        return contentStringResponse;
    }
}
