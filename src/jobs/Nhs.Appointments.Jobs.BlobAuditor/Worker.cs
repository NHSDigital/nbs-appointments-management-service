using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;

namespace Nhs.Appointments.Jobs.BlobAuditor;
public class Worker<T>(string dataType, IAuditChangeFeedHandler<T> handler) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = await handler.ResolveChangeFeedForContainer(dataType);

        await processor.StartAsync();
    }
}
