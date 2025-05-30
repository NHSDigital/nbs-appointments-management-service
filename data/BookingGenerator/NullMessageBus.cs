using Nhs.Appointments.Core.Messaging;

namespace BookingGenerator;

public class NullMessageBus : IMessageBus
{
    public Task Send<T>(params T[] messages) where T : class => Task.CompletedTask;
}
