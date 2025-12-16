using Azure.Storage.Blobs;

namespace Nhs.Appointments.Jobs.BlobAuditor.Blob;

public class AzureBlobStorage(BlobServiceClient blobServiceClient) : IAzureBlobStorage
{
    public async Task<Stream> GetBlobUploadStream(string containerName, string blobName)
    {
        var containerClient = ResolveContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
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
