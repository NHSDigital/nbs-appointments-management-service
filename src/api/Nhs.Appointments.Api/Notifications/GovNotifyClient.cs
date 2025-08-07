using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyClient(IAsyncNotificationClient client, IPrivacyUtil privacy, IOptions<GovNotifyRetryOptions> retryOptions) : ISendNotifications
{
    private async Task RetryAsync(Func<Task> action)
    {
        var delay = retryOptions.Value.InitialDelayMs;
        for (var attempt = 1; attempt <= retryOptions.Value.MaxRetries; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (NotifyClientException) when (attempt < retryOptions.Value.MaxRetries)
            {
                await Task.Delay(delay);
                delay = (int)(delay * retryOptions.Value.BackoffFactor);
            }
        }
    }

    public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));

        try
        {
            await RetryAsync(() =>
                client.SendEmailAsync(emailAddress, templateId, templateValues)
            );
        }
        catch (NotifyClientException ex)
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
            await RetryAsync(() =>
                client.SendSmsAsync(phoneNumber, templateId, templateValues)
            );
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

