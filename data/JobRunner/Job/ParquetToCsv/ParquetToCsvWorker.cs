using JobRunner.Job.Notify;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Api.Notifications;
using Nhs.Appointments.Core.Messaging.Events;

namespace JobRunner.Job.ParquetToCsv;

public class ParquetToCsvWorker(
    IHostApplicationLifetime hostApplicationLifetime,
    IAzureBlobStorage azureBlobStorage,
    INotifyInfoReader<BookingInfo> bookingInfoReader,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var bookingFileStream = await azureBlobStorage.GetBlob("notify", "bookings_to_notify.parquet");
            
            var bookings = (await bookingInfoReader.ReadStreamAsync(bookingFileStream)).ToArray();

            await ProcessBookings(NotificationType.Sms, bookings);
            await ProcessBookings(NotificationType.Email, bookings);

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
            Destination = notificationType == NotificationType.Sms ? x.PHONE_NUMBER : x.EMAIL, Type = notificationType, Name = x.FIRST_NAME
        }).Where(x => !string.IsNullOrEmpty(x.Destination)).Chunk(50000);

        var iterator = 1;
        
        foreach (var notification in notifications)
        {
            var stream = await azureBlobStorage.GetBlobUploadStream("csvnotify", $"{(notificationType == NotificationType.Sms ? "sms" : "email")}-notifications-{iterator}.csv");
            await using var streamWriter = new StreamWriter(stream);
            await WriteToCsv(streamWriter, notificationType, notification);
            iterator++;
        }
    }

    private async Task WriteToCsv(TextWriter csvWriter, NotificationType type, Notification[] rows)
    {
        await csvWriter.WriteLineAsync(string.Join(',', [$"{(type == NotificationType.Sms ? "phone number" : "email address")}", "firstName"]));
        foreach (var row in rows)
        {
            await csvWriter.WriteLineAsync(string.Join(',', [CsvStringValue(row.Destination), CsvStringValue(row.Name)]));
        }
    }


    private static string CsvStringValue(string value) => "\"" + value + "\"";
}


public class Notification
{
    public string Destination { get; set; }
    public NotificationType Type { get; set; }
    public string Name { get; set; }
}
