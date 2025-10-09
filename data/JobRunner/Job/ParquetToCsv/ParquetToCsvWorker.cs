using System.Net.Mail;
using System.Text.RegularExpressions;
using JobRunner.Job.Notify;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;
using Parquet;

namespace JobRunner.Job.ParquetToCsv;

public class ParquetToCsvWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IAzureBlobStorage azureBlobStorage,
    INotifyInfoReader<BookingInfo> bookingInfoReader,
    ISendTracker sendTracker,
    IOptions<JobOptions> jobOptions,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var bookingFileStream = await azureBlobStorage.GetBlob("notify", "bookings_to_notify.parquet");
            await sendTracker.RefreshState($"{jobOptions.Value.Environment}-{jobOptions.Value.Notification}-tracker.parquet");
            
            var bookings = (await bookingInfoReader.ReadStreamAsync(bookingFileStream)).ToArray();

            await ProcessBookings(NotificationType.Sms, bookings);
            await ProcessBookings(NotificationType.Email, bookings);
            
            await sendTracker.Persist($"{jobOptions.Value.Environment}-{jobOptions.Value.Notification}-tracker.parquet");
        }
        catch (Exception ex)
        {
            logger.LogError(ex,ex.ToString());
            Environment.ExitCode = -1;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task ProcessBookings(NotificationType notificationType, IEnumerable<BookingInfo> bookings)
    {
        var notifications = bookings.Select(x => new Notification()
        {
            Destination = notificationType == NotificationType.Sms ? x.PHONE_NUMBER : x.EMAIL, Type = notificationType, 
            Name = x.FIRST_NAME, 
            Reference = x.BOOKING_REFERENCE_NUMBER
        }).Chunk(50000);

        var iterator = 1;
        
        foreach (var notification in notifications)
        {
            var stream = await azureBlobStorage.GetBlobUploadStream("csvnotify", $"{(notificationType == NotificationType.Sms ? "sms" : "email")}-notifications-{iterator}.csv");
            await using var streamWriter = new StreamWriter(stream);
            await WriteToCsv(streamWriter, notificationType, notification);
            iterator++;
        }
    }

    private bool DestinationValid(NotificationType notificationType, string destination)
    {
        switch (notificationType)
        {
            case NotificationType.Sms:
                var tester = new Regex("^\\+?(?:[0-9] ?){6,14}[0-9]$");
                return tester.IsMatch(destination);
            case NotificationType.Email:
                try
                {
                    _ = new MailAddress(destination);
                    return true;
                }
                catch
                {
                    return false;
                }
            default:
                throw new InvalidOperationException($"Invalid notification type {notificationType}");
        }
    }

    private async Task WriteToCsv(TextWriter csvWriter, NotificationType type, Notification[] rows)
    {
        var typeAsString = type == NotificationType.Email ? "Email" : type == NotificationType.Sms ? "Sms" : "Unknown";
        await csvWriter.WriteLineAsync(string.Join(',', [$"{(type == NotificationType.Sms ? "phone number" : "email address")}", "firstName"]));
        foreach (var row in rows)
        {
            if (await sendTracker.HasSuccessfulSent(row.Reference, typeAsString, jobOptions.Value.Notification))
            {
                logger.LogInformation($"Skipping {row.Reference} - {typeAsString} as it has already been sent");
                continue;
            }

            var message = string.Empty;
            var success = false;
            
            if (DestinationValid(type, row.Destination))
            {
                await csvWriter.WriteLineAsync(string.Join(',',
                    [CsvStringValue(row.Destination), CsvStringValue(row.Name)]));
                success = true;
                await sendTracker.RecordSend(
                    row.Reference, 
                    typeAsString, 
                    jobOptions.Value.Notification, 
                    true, 
                    string.Empty);
            }
            else
            {
                success = false;
                message = "Invalid Destination";
            }
            
            await sendTracker.RecordSend(
                row.Reference, 
                typeAsString, 
                jobOptions.Value.Notification, 
                success, 
                message);
        }
    }


    private static string CsvStringValue(string value) => "\"" + value + "\"";
}


public class Notification
{
    public string Destination { get; set; }
    public NotificationType Type { get; set; }
    public string Name { get; set; }

    public string Reference { get; set; }
}
