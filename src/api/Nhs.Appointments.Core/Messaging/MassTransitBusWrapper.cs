using MassTransit;

namespace Nhs.Appointments.Core.Messaging;

public class MassTransitBusWrapper(IBus bus) : IMessageBus
{
    public async Task Send<T>(T message) where T : class
    {
       await bus.Send(message);
    }
}
