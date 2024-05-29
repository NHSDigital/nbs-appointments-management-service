using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Options;
using Polly;

namespace Nhs.Appointments.Core.Concurrency;

internal class AzureStorageSiteLeaseManager : ISiteLeaseManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly SiteLeaseManagerOptions _options;

    public AzureStorageSiteLeaseManager(IOptions<SiteLeaseManagerOptions> options, BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        _options = options.Value;
    }

    public ISiteLeaseContext Acquire(string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        containerClient.CreateIfNotExists();

        var blobClient = containerClient.GetBlobClient(fileName);
        if (blobClient.Exists() == false)
            blobClient.Upload(BinaryData.FromString(""));

        var leaseClient = blobClient.GetBlobLeaseClient();
        var leasePipeline = CreateResiliencePipeline();
        leasePipeline.Execute(() => {
            return leaseClient.Acquire(TimeSpan.FromSeconds(20));
        });

        return new SiteLeaseContext(() => leaseClient.Release());
    }

    private ResiliencePipeline<Azure.Response<BlobLease>> CreateResiliencePipeline()
    {
        return new ResiliencePipelineBuilder<Azure.Response<BlobLease>>()
            .AddRetry(new Polly.Retry.RetryStrategyOptions<Azure.Response<BlobLease>>
            {
                ShouldHandle = arguments => arguments.Outcome switch
                {
                    { Exception: Azure.RequestFailedException ex } when ex.ErrorCode == "LeaseAlreadyPresent" => PredicateResult.True(),
                    _ => PredicateResult.False(),
                },
                MaxRetryAttempts = 20,
                Delay = TimeSpan.FromMilliseconds(100)
            })
            .Build();
    }                
}

public class SiteLeaseContext : ISiteLeaseContext
{
    private readonly Action _release;

    public SiteLeaseContext(Action release) 
    {
        _release = release;
    }

    public void Dispose()
    {
        _release();
    }
}
