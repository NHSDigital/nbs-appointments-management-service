using BookingsDataExtracts;
using DataExtract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;
using FileOptions = DataExtract.FileOptions;

namespace BookingDataExtracts.Integration;

public class TestableDataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    TimeProvider timeProvider,
    IOptions<FileOptions> fileOptions,
    IServiceProvider serviceProvider,
    IFileSender fileSender,
    ILogger<TestableDataExtractWorker> logger
    ) : DataExtractWorker<BookingDataExtract>(hostApplicationLifetime, timeProvider,
        fileOptions, serviceProvider, fileSender, logger)

{
    public Task Test()
    {
        return ExecuteAsync(new CancellationToken());
    }
}
