using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class BookingMadeNotifier(ISendNotifications notificationClient, INotificationConfigurationStore notificationConfigurationStore, ISiteSearchService siteService) : IBookingMadeNotifier
{
    public async Task Notify(string eventType, string service, string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, bool emailConsent, string email, bool phoneConsent, string phoneNumber)
    {
        var site = await siteService.GetSiteByIdAsync(siteId);

        if(site == null)
        {
            throw new Exception($"The site '{siteId}' was not found. No booking notification could be sent.");
        }

        var templateValues = new Dictionary<string, dynamic>
        {
            {"firstName", firstName},
            {"siteName", site.Name },
            {"date", date.ToLongDateString() },
            {"time", time.ToString("HH:mm") },
            {"reference", bookingRef},
            {"address", site.Address }
        };

        var notificationConfig = await notificationConfigurationStore.GetNotificationConfigurationForService(service, eventType);

        if(emailConsent && !string.IsNullOrEmpty(email))
        {
            await notificationClient.SendEmailAsync(email, notificationConfig.EmailTemplateId, templateValues);
        }

        if(phoneConsent && !string.IsNullOrEmpty(phoneNumber)) 
        {
            await notificationClient.SendSmsAsync(phoneNumber, notificationConfig.SmsTemplateId, templateValues);
        }
    }
}
