using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace DataExtract;
public class BlobFileSender(
        IOptions<BlobFileOptions> options,
        BlobServiceClient blobServiceClient) : IFileSender
{

    public async Task SendFile(FileInfo file)
    {
        await SendToBlob(file.Name, options.Value.ContainerName, file.FullName);

        Console.WriteLine($"Uploaded {file.Name} to blob at {options.Value.ContainerName}");
    }

    private async Task SendToBlob(string fileName, string containerName, string localPath)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(localPath, overwrite: true);
    }
}
