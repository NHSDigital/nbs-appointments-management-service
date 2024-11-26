using MassTransit;

namespace Nhs.Appointments.Core.Messaging;

public class MassTransitBusWrapper(IBus bus) : IMessageBus
{
    public async Task Send<T>(params T[] messages) where T : class
    {
       foreach(T message in messages)
        {
            await bus.Send(message);
        }
    }
}
