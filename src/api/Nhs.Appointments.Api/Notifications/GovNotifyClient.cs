using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications.Options;
using Notify.Client;
using Notify.Exceptions;
using Notify.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyClient(IAsyncNotificationClient client, IPrivacyUtil privacy, IOptions<GovNotifyRetryOptions> retryOptions) : ISendNotifications
{
    private async Task RetryOnExceptionAsync<TException>(Func<Task> action) where TException : Exception
    {
        var delay = retryOptions.Value.InitialDelayMs;
        for (var attempt = 1; attempt <= retryOptions.Value.MaxRetries; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (TException ex) when (attempt < retryOptions.Value.MaxRetries)
            {
                // the following pattern is from GovNotify
                // error handling suggestions:
                // https://docs.notifications.service.gov.uk/net.html#error-handling
                var pattern = """(?<=Status code )([0-9]+)""";
                var r = new Regex(pattern, RegexOptions.IgnoreCase);
                var match = r.Match(ex.Message);
                var statusCode = int.Parse(match.Value);
                if (statusCode == 429)
                {
                    await Task.Delay(delay);
                    delay = (int)(delay * retryOptions.Value.BackoffFactor);
                }
                else 
                {
                    throw;
                }
            }
        }
    }

    public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));

        try
        {
            await RetryOnExceptionAsync<NotifyClientException>(() =>
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
            await RetryOnExceptionAsync<NotifyClientException>(() =>
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

