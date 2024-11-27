namespace Nhs.Appointments.Core.Messaging;

public interface IMessageBus
{
    Task Send<T>(params T[] messages) where T : class;
}
