using BookingsDataExtracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nbs.MeshClient;
using Nbs.MeshClient.Auth;

namespace BookingDataExtractTests;

public class TestableDataExtractWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IOptions<MeshSendOptions> meshSendOptions,
    IOptions<MeshAuthorizationOptions> meshAuthOptions,
    IMeshFactory meshFactory,
    BookingDataExtract bookingDataExtract
    ) : DataExtractWorker(hostApplicationLifetime, meshSendOptions, meshAuthOptions, meshFactory, bookingDataExtract)
{
    public Task Test()
    {
        return ExecuteAsync(new CancellationToken());
    }
}
