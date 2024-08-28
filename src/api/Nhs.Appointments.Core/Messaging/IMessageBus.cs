using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nhs.Appointments.Core.Messaging
{
    public interface IMessageBus
    {
        Task Send<T>(T message);
    }
}
