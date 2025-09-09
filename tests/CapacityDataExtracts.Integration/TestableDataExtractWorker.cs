using CapacityDataExtracts;
using DataExtract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileOptions = DataExtract.FileOptions;

namespace CapacityDataExtracts.Integration;

public class TestableDataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    TimeProvider timeProvider,
    IOptions<FileOptions> fileOptions,
    IServiceProvider serviceProvider,
    IFileSender fileSender,
    ILogger<TestableDataExtractWorker> logger
) : DataExtractWorker<CapacityDataExtract>(hostApplicationLifetime, timeProvider,
    fileOptions, serviceProvider, fileSender, logger)

{
    public Task Test()
    {
        return ExecuteAsync(new CancellationToken());
    }
}
