using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface IBookingNotifier
{
    Task Notify(string eventType, string service, string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, NotificationType type, string destination);
}
