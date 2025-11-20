using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace CosmosAuditor.Blob;

public class AzureBlobStorage(BlobServiceClient blobServiceClient) : IAzureBlobStorage
{
    public async Task<Stream?> GetBlob(string containerName, string blobName)
    {
        var containerClient = ResolveContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var content = await blobClient.DownloadContentAsync();
        return content.Value.Content.ToStream();

    }

    public async Task<Stream> GetBlobUploadStream(string containerName, string blobName)
    {
        var containerClient = ResolveContainerClient(containerName);
        var blobClient = containerClient.GetBlockBlobClient(blobName);
        return await blobClient.OpenWriteAsync(true);
    }

    private BlobContainerClient ResolveContainerClient(string containerName)
    {
        var containers = blobServiceClient.GetBlobContainers();
        
        return containers.Any(x => x.Name.Equals(containerName)) 
            ? blobServiceClient.GetBlobContainerClient(containerName) 
            : blobServiceClient.CreateBlobContainer(containerName);
    }
}
