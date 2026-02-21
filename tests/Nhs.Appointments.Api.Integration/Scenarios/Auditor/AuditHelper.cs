using Azure.Storage.Blobs;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Integration.Scenarios.Auditor;

public class AuditHelper
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private TimeSpan delay => TimeSpan.FromSeconds(1);
    private TimeSpan timeout => TimeSpan.FromSeconds(10);

    public AuditHelper()
    {
        _blobServiceClient = new BlobServiceClient(ConnectionString);
    }

    public async Task<string?> PollForAuditLogAsync(string containerSource, string fileName)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            var content = await ReadAuditJsonByNameAsync(containerSource, fileName);

            if (content != null)
            {
                return content;
            }

            await Task.Delay(delay);
        }

        return null;
    }

    public string GetBlobName(
        string documentType, 
        DateTimeOffset timeStamp,
        string identifier

    ) => Path.Combine(
        $"{timeStamp:yyyyMMdd}",
        documentType,
        $"{timeStamp:yyyyMMddHHmmssfff}-{identifier}.json"
    );

    private async Task<string?> ReadAuditJsonByNameAsync(string containerSource, string fileName)
    {
        var containerName = $"{DateTime.UtcNow:yyyyMMdd}-{containerSource.Replace("_", "")}";
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync()) return null;

        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }
}
