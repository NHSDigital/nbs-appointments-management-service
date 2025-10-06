namespace JobRunner.Job.Notify;

public class LocalBlobStorage : IAzureBlobStorage
{
    public Task<Stream?> GetBlob(string containerName, string blobName)
    {
        var target = Path.Combine(containerName, blobName);

        
        if (!File.Exists(target))
        {
            return Task.FromResult<Stream?>(null);
        }

        return Task.FromResult<Stream?>(File.OpenRead(target));
    }

    public Task<Stream> GetBlobUploadStream(string containerName, string blobName)
    {
        var target = Path.Combine(containerName, blobName);
        Directory.CreateDirectory(containerName);
        
        return Task.FromResult<Stream>(File.OpenWrite(target));
    }
}
