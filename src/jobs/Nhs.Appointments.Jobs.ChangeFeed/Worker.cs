using Microsoft.Extensions.Hosting;

namespace Nhs.Appointments.Jobs.ChangeFeed;

public class Worker<T>(string dataType, IChangeFeedHandler handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = await handler.ResolveChangeFeedForContainer(dataType);

        await processor.StartAsync();
    }
}
