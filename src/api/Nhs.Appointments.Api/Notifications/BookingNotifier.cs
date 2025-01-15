using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class BookingNotifier(
    ISendNotifications notificationClient, 
    INotificationConfigurationService notificationConfigurationService,
    ISiteService siteService, 
    IPrivacyUtil privacy,
    ILogger logger) : IBookingNotifier
{
    public async Task Notify(string eventType, string service, string bookingRef, string siteId, string firstName, DateOnly date, TimeOnly time, NotificationType notificationType, string destination)
    {
        if(notificationType == NotificationType.Unknown)
        {
            logger.LogWarning($"Unable to send notification as event type is unknown");
            return;
        }

        if (ValidateDestination(notificationType, destination) == false)
        {            
            logger.LogWarning($"Unable to send notification for {eventType}:{bookingRef} to {notificationType}:{destination} because the destination is invalid");
            return;
        }

        var notificationConfiguration = await notificationConfigurationService.GetNotificationConfigurationsAsync(eventType, service);
        if (notificationConfiguration == null)
        {            
            logger.LogWarning($"Unable to send notification for {eventType}:{bookingRef} to {notificationType}:{destination} because there is no notiftications configured for the event for the specific service");
            return;
        }

        var site = await siteService.GetSiteByIdAsync(siteId);
        if(site == null)
        {
            logger.LogWarning($"Unable to send notification for {eventType}:{bookingRef} to {notificationType}:{destination} because the specified site does not exist");
            return;
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

        var templateId = GetTemplateId(notificationConfiguration, notificationType);
        await SendNotification(notificationType, destination, templateId, templateValues);
    }

    private string GetTemplateId(NotificationConfiguration config, NotificationType notificationType) => notificationType switch
    {
        NotificationType.Email => config.EmailTemplateId,
        NotificationType.Sms => config.SmsTemplateId,
        _ => throw new NotSupportedException("Unknown notification type")
    };

    const string MobileNumberRegex = @"^\s*07(?=\d{9,11}(\s*)$)[\d]+(?:\s[\d]+)*\s*$|^(?(?=(\s*(\+\(44\)|\(\+44\)|\+44)))\s*(\+\(44\)|\(\+44\)|\+44)\s?((07|7)(?=\d{9,11}(\s*)$)[\d]+(?:\s[\d]+)*\s*)|(?(?=(\s*(\+\(\d{2}\)|\(\+\d{2}\)|\+\d{2})))(\s*(\+\(\d{2}\)|\(\+\d{2}\)|\+\d{2}))\s?(?=.{8,19}(\s*)$)[\d]+(?:\s[\d]+)*\s*))$";
    const string EmailAddressRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";    

    private bool ValidateDestination(NotificationType type, string destination)
    {
        if(String.IsNullOrEmpty(destination))
            return false;

        switch(type)
        {
            case NotificationType.Email: return Regex.Match(destination, EmailAddressRegex).Success;
            case NotificationType.Sms: return Regex.Match(destination, MobileNumberRegex).Success;
            default: throw new NotSupportedException();
        }
    }

    private Task SendNotification(NotificationType notificationType, string destination, string templateId, Dictionary<string, dynamic> templateValues) => notificationType switch
    {
        NotificationType.Email => notificationClient.SendEmailAsync(destination, templateId, templateValues),
        NotificationType.Sms => notificationClient.SendSmsAsync(destination, templateId, templateValues),
        _ => throw new NotSupportedException("Unknown notification type")
    };

    private string ObfuscateDestination(NotificationType type, string destination) => type switch
    {
        NotificationType.Email => privacy.ObfuscateEmail(destination),
        NotificationType.Sms => privacy.ObfuscatePhoneNumber(destination),
        _ => throw new NotSupportedException("Unknown notification type")
    };    
}
