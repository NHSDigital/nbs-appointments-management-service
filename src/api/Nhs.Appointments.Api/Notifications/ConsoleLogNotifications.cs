using Nhs.Appointments.Api.Messaging;
using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications
{
    public class ConsoleLogNotifications : IMessageBus
    {
        public Task Send<T>(T message)
        {
            Console.WriteLine(Json.JsonResponseWriter.Serialize(message));
            return Task.CompletedTask;
        }
    }
}
