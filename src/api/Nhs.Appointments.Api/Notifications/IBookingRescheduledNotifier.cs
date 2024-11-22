using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface IBookingRescheduledNotifier
{
    Task Notify(string eventType, string service, string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, string email, string phoneNumber);
}
