using MassTransit;

namespace Nhs.Appointments.Core.Messaging
{
    public class MassTransitBusWrapper : IMessageBus
    {
        private readonly IBus _bus;

        public MassTransitBusWrapper(IBus bus)
        {
            _bus = bus;
        }

        public Task Send<T>(T message) => _bus.Send(message);
    }
}
