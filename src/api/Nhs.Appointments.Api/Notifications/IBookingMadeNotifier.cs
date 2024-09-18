using System;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface IBookingMadeNotifier
{
    Task Notify(string eventType, string service, string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, bool emailConsent, string email, bool phoneConsent, string phoneNumber);
}
