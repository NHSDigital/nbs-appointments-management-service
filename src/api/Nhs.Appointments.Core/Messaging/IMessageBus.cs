namespace Nhs.Appointments.Core.Messaging;

public interface IMessageBus
{
    Task Send<T>(T message);
}
