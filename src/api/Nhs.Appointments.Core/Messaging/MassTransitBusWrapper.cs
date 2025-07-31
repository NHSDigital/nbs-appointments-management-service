using MassTransit;

namespace Nhs.Appointments.Core.Messaging;

public class MassTransitBusWrapper(IBus bus) : IMessageBus
{
    public async Task Send<T>(params T[] messages) where T : class
    {
        EndpointConvention.TryGetDestinationAddress<T>(out var destinationAddress);
        var endpoint = await bus.GetSendEndpoint(destinationAddress).ConfigureAwait(false);

        await bus.PublishBatch(messages);
        
        await endpoint.SendBatch(messages);
    }
}
