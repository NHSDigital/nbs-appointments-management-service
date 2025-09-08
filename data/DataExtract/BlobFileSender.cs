using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace DataExtract;
public class BlobFileSender(
        IOptions<BlobFileOptions> options,
        BlobServiceClient blobServiceClient) : IFileSender
{

    public async Task SendFile(FileInfo file)
    {
        var container = options.Value.ContainerName.ToLower();
        await SendToBlob(file.Name, container, file.FullName);

        Console.WriteLine($"Uploaded {file.Name} to blob at {container}");
    }

    private async Task SendToBlob(string fileName, string containerName, string localPath)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(localPath, overwrite: true);
    }
}
