using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace Nhs.Appointments.Api.Features;

public class LocalConfigurationRefresher : IConfigurationRefresher
{
    public Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void ProcessPushNotification(PushNotification pushNotification, TimeSpan? maxDelay = null) { }

    public Uri AppConfigurationEndpoint => new(string.Empty);

    public Task RefreshAsync(CancellationToken cancellationToken = new()) => Task.CompletedTask;
}
