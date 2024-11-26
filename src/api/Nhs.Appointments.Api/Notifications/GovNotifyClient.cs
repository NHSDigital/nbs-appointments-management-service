using Notify.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyClient(Notify.Client.NotificationClient client, IPrivacyUtil privacy) : ISendNotifications
{
    public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));

        try
        {
            await client.SendEmailAsync(emailAddress, templateId, templateValues);
        }
        catch(NotifyClientException ex)
        {
            throw new NotificationException($"Gov Notify rejected the attempt to send notification email to {privacy.ObfuscateEmail(emailAddress)} using template id {templateId}.", ex);
        }
        catch (Exception ex)
        {
            throw new NotificationException($"Failed to send notification email to {privacy.ObfuscateEmail(emailAddress)} using template id {templateId}.", ex);
        }
    }

    public async Task SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));
        
        try
        {
            await client.SendSmsAsync(phoneNumber, templateId, templateValues);
        }
        catch (NotifyClientException ex)
        {
            throw new NotificationException($"Gov Notify rejected the attempt to send notification SMS to {privacy.ObfuscatePhoneNumber(phoneNumber)} using template id {templateId}.", ex);
        }
        catch (Exception ex)
        {
            throw new NotificationException($"Failed to send notification SMS to {privacy.ObfuscatePhoneNumber(phoneNumber)} using template id {templateId}.", ex);
        }
    }
}

