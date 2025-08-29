using DataExtract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using FileOptions = DataExtract.FileOptions;

namespace CapacityDataExtracts.Integration;

public class TestableDataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory,
    TimeProvider timeProvider,
    CapacityDataExtract bookingDataExtract,
    IOptions<FileOptions> fileOptions,
    IOptions<FileSenderOptions> fileSenderOptions,
    IFileSenderFactory fileSenderFactory,
    IServiceProvider serviceProvider
    ) : DataExtractWorker<CapacityDataExtract>(hostApplicationLifetime, meshSendOptions, meshAuthOptions, meshFactory, timeProvider,
        bookingDataExtract, fileOptions, fileSenderOptions, fileSenderFactory, serviceProvider)
{
    public Task Test()
    {
        return ExecuteAsync(new CancellationToken());
    }
}
