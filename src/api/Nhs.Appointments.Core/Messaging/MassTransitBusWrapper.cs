using MassTransit;

namespace Nhs.Appointments.Core.Messaging;

public class MassTransitBusWrapper(IBus bus) : IMessageBus
{
    public async Task Send<T>(params T[] messages) where T : class
    {
        if (!EndpointConvention.TryGetDestinationAddress<T>(out var destinationAddress))
        {
            throw new ArgumentException($"A convention for the message type {TypeCache<T>.ShortName} was not found");
        }

        var endpoint = await bus.GetSendEndpoint(destinationAddress).ConfigureAwait(false);

        await endpoint.SendBatch(messages);
    }
}
