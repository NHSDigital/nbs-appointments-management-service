using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class BookingMadeNotifier(ISendNotifications notificationClient, ISiteSearchService siteService, IOptions<BookingMadeNotifier.Options> options) : IBookingMadeNotifier
{
    public async Task Notify(string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, bool emailConsent, string email, bool phoneConsent, string phoneNumber)
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

        if(emailConsent && !string.IsNullOrEmpty(email))
        {
            await notificationClient.SendEmailAsync(email, options.Value.EmailTemplateId, templateValues);
        }

        if(phoneConsent && !string.IsNullOrEmpty(phoneNumber)) 
        {
            await notificationClient.SendSmsAsync(phoneNumber, options.Value.SmsTemplateId, templateValues);
        }
    }
    public class Options
    {
        public string EmailTemplateId { get; set; }
        public string SmsTemplateId { get; set; }
    }
}
