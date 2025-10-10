using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications.Options;
using Notify.Exceptions;
using Notify.Interfaces;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyClient(
    IAsyncNotificationClient client, 
    IPrivacyUtil privacy, 
    IOptions<GovNotifyRetryOptions> retryOptions,    
    ILogger<GovNotifyClient> logger) : ISendNotifications
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
            catch (TException ex) when (attempt <= retryOptions.Value.MaxRetries)
            {
                var errorStatusCode = ParseGovNotifyExceptionMessage(ex.Message);
                if (errorStatusCode != null)
                {
                    if (errorStatusCode == 429)
                    {
#pragma warning disable S6667 // Logging in a catch clause should pass the caught exception as a parameter.
                        logger.LogWarning(ex, "Received 429 Too Many Requests. Retrying...");
#pragma warning restore S6667 // Logging in a catch clause should pass the caught exception as a parameter.
                        await Task.Delay(delay);
                        delay = (int)(delay * retryOptions.Value.BackoffFactor);
                    }
                    else
                    {
                        logger.LogError(ex, "Non-retryable status code {errorStatusCode}. Aborting.", errorStatusCode);
                        break;
                    }
                }
                else
                {
                    logger.LogError(ex, "Could not parse status code from exception message. Aborting.");
                    break;
                }
            }
        }
    }

    public int? ParseGovNotifyExceptionMessage(string message)
    {
        // the following pattern is from GovNotify
        // error handling suggestions:
        // https://docs.notifications.service.gov.uk/net.html#error-handling
        var pattern = """(?<=Status code )([0-9]+)""";
        var timeout = TimeSpan.FromSeconds(2);
        var r = new Regex(pattern, RegexOptions.IgnoreCase, timeout);
        var match = r.Match(message);

        if (match.Success && int.TryParse(match.Value, out var statusCode))
        {
            return statusCode;
        }

        return null;
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

