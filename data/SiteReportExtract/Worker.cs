using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nhs.Appointments.Core;

namespace SiteReportExtract;

public class Worker(
    IHostApplicationLifetime hostApplicationLifetime,
    IConfiguration configuration, 
    ISiteService siteService, 
    ISiteReportService siteReportService,
    TimeProvider timeProvider,
    BlobServiceClient blobServiceClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var startDate = configuration.GetValue<DateOnly>("StartDate");
            var endDate = configuration.GetValue<DateOnly>("EndDate");
            var sites = await siteService.GetAllSites();
            var fileName =
                $"SiteReport_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{timeProvider.GetUtcNow():yyyyMMddhhmmss}.csv";
        
            var report = await siteReportService.Generate(sites.ToArray(), startDate, endDate);

            await SendReport(fileName, report.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Environment.ExitCode = -1;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
        
    }
    
    private async Task SendReport(string fileName, SiteReport[] rows)
    {
        var result = ProcessToFile(new StringBuilder(), rows);
        await SendToBlob(fileName, result);
    }

    private async Task SendToBlob(string fileName, string document)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient("adhocreports");
        await containerClient.CreateIfNotExistsAsync();

        await containerClient.UploadBlobAsync(fileName, BinaryData.FromString(document));
    }

    private string ProcessToFile(StringBuilder csvWriter, SiteReport[] rows)
    {
        var distinctServices = rows.SelectMany(x => x.Bookings.Keys).Union(rows.SelectMany(x => x.RemainingCapacity.Keys))
            .Distinct().ToArray();
        
        csvWriter.AppendLine(string.Join(',', SiteReportMap.Headers(distinctServices)));
        foreach (var row in rows)
        {
            csvWriter.AppendLine(string.Join(',', [
                SiteReportMap.SiteName(row),
                SiteReportMap.ICB(row),
                SiteReportMap.Region(row),
                SiteReportMap.OdsCode(row),
                SiteReportMap.Longitude(row),
                SiteReportMap.Latitude(row),
                string.Join(',', distinctServices.Select(service => SiteReportMap.BookingsCount(row, service))),
                SiteReportMap.TotalBookings(row).ToString(),
                SiteReportMap.Cancelled(row).ToString(),
                SiteReportMap.Orphaned(row).ToString(),
                SiteReportMap.MaximumCapacity(row).ToString(),
                string.Join(',', distinctServices.Select(service => SiteReportMap.CapacityCount(row, service)))
            ]));
        }

        return csvWriter.ToString();
    }
}


public static class SiteReportMap
{
    public static string[] Headers(string[] services)
    {
        var siteHeaders = new[] { "Site Name", "ICB", "Region", "ODS Code", "Longitude", "Latitude" };
        var statHeaders = new[] { "Total Bookings", "Cancelled", "Orphaned", "Maximum Capacity" };
        var bookingsHeaders = services.Select(service => $"{service} Booked");
        var capacityHeaders = services.Select(service => $"{service} Capacity");

        return siteHeaders
            .Union(bookingsHeaders
                .Union(statHeaders))
            .Union(capacityHeaders)
            .ToArray();
    } 
    public static string SiteName(SiteReport report) => report.SiteName;
    public static string ICB(SiteReport report) => report.ICB;
    public static string Region(SiteReport report) => report.Region;
    public static string OdsCode(SiteReport report) => report.OdsCode;
    public static double Longitude(SiteReport report) => report.Longitude;
    public static double Latitude(SiteReport report) => report.Latitude;
    public static int TotalBookings(SiteReport report) => report.TotalBookings;
    public static int Cancelled(SiteReport report) => report.Cancelled;
    public static int Orphaned(SiteReport report) => report.Orphaned;
    public static int MaximumCapacity(SiteReport report) => report.MaximumCapacity;
    public static int BookingsCount(SiteReport report, string key) => report.Bookings.TryGetValue(key, out var booking) ? booking : 0;
    public static int CapacityCount(SiteReport report, string key) => report.RemainingCapacity.TryGetValue(key, out var capacity) ? capacity : 0;
}
