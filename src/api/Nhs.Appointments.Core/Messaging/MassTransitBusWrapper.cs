using MassTransit;

namespace Nhs.Appointments.Core.Messaging;

public class MassTransitBusWrapper(IBus bus) : IMessageBus
{
    public Task Send<T>(T message) => bus.Send(message);
}
