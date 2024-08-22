using MassTransit;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Messaging
{
    public interface IMessageBus
    {
        Task Send<T> (T message);
    }
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
