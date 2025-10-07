using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace JobRunner.Job.Notify;

public class Worker(
    IHostApplicationLifetime hostApplicationLifetime,
    IAzureBlobStorage azureBlobStorage,
    INotifyInfoReader<BookingInfo> bookingInfoReader,
    ISendTracker sendTracker,
    ISendNotifications  sendNotifications,
    IOptions<NotifySendOptions> notifySendOptions,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var bookingFileStream = await azureBlobStorage.GetBlob("notify", "bookings_to_notify.parquet");
            if (bookingFileStream is null)
            {
                throw new FileNotFoundException("No booking file found 'notify/bookings_to_notify'");
            }

            var bookingInfos = (await bookingInfoReader.ReadStreamAsync(bookingFileStream)).ToArray();
            
            logger.LogInformation($"Downloaded booking info, total bookings to notify - {bookingInfos.Length}");

            var batches = bookingInfos.Chunk(3);
            var totalBatches = batches.Count();
            var iterations = 1;
            
            foreach (var batch in batches)
            {
                logger.LogInformation($"Start a batch");
                logger.LogInformation("Refreshing Send Tracker");
                await sendTracker.RefreshState("adhocSend.parquet");

                foreach (var bookingInfo in batch)
                {
                    logger.LogInformation($"Sending notifications for {bookingInfo.BOOKING_REFERENCE_NUMBER}");
                    var templateValues = new Dictionary<string, dynamic>()
                    {
                        { "firstName", bookingInfo.FIRST_NAME }
                    };

                    var tasks = new List<Task>
                    {
                        SendNotification(bookingInfo.BOOKING_REFERENCE_NUMBER, NotificationType.Email, bookingInfo.EMAIL,
                            notifySendOptions.Value.EmailTemplateId, templateValues),
                        SendNotification(bookingInfo.BOOKING_REFERENCE_NUMBER, NotificationType.Sms, bookingInfo.PHONE_NUMBER,
                            notifySendOptions.Value.PhoneTemplateId, templateValues)
                    };

                    await Task.WhenAll(tasks);
                }

                logger.LogInformation($"Finished batch, persisting state and waiting");

                await sendTracker.Persist("adhocSend.parquet");
                if (iterations < totalBatches)
                { 
                    await Task.Delay(60000, stoppingToken); // wait to avoid 429 for next batch
                }
                iterations++;
            }
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex,ex.ToString());
            Environment.ExitCode = -1;
        }
        finally
        {
            await sendTracker.Persist("adhocSend.parquet");
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task SendNotification(string reference, NotificationType notificationType, string destination, string templateId,
        Dictionary<string, dynamic> templateValues)
    {
        var notificationTypeString = notificationType == NotificationType.Email ? "Email" :
            notificationType == NotificationType.Sms ? "Sms" : "Unknown";

        if (await sendTracker.HasSuccessfulSent(reference, notificationTypeString, templateId))
        {
            logger.LogInformation($"Already sent notification for {reference} to {notificationTypeString}. SKIPPING");
            return;
        }

        if (destination.IsNullOrWhiteSpace())
        {
            await sendTracker.RecordSend(reference, notificationType.ToDisplayName(), templateId, false, "Empty destination");
            return;
        }

        var success = false;
        var message = string.Empty;

        try
        {
            switch (notificationType)
            {
                case NotificationType.Email:
                    await sendNotifications.SendEmailAsync(destination, templateId, templateValues);
                    success = true;
                    break;
                case NotificationType.Sms:
                    await sendNotifications.SendSmsAsync(destination, templateId, templateValues);
                    success = true;
                    break;
                case NotificationType.Unknown:
                default:
                    throw new NotSupportedException("Unknown notification type");
            }
        }
        catch (Exception ex)
        {
            success = false;
            message = ex.Message;
        }
        finally
        {
            await sendTracker.RecordSend(
                reference, 
                notificationType == NotificationType.Email ? "Email" : notificationType == NotificationType.Sms ? "Sms" : "Unknown", 
                templateId, 
                success, 
                message);
        }


    }
}
