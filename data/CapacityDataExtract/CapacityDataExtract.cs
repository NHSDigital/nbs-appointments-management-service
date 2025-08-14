using CapacityDataExtracts.Documents;
using DataExtract;
using Nhs.Appointments.Persistance.Models;
using Parquet.Serialization;

namespace CapacityDataExtracts;

public class CapacityDataExtract(
    CosmosStore<DailyAvailabilityDocument> availabilityStore,
    CosmosStore<SiteDocument> sitesStore,
    TimeProvider timeProvider) : IExtractor
{
    public async Task RunAsync(FileInfo outputFile)
    {
        var availabilityTask = availabilityStore.RunQueryAsync(
            b => b.DocumentType == "daily_availability"
                && b.Date >= DateOnly.FromDateTime(timeProvider.GetUtcNow().Date)
                && b.Date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().Date.AddDays(90)),
            b => b
        );

        var siteTask = sitesStore.RunQueryAsync(s => s.DocumentType == "site", s => s);
        
        await Task.WhenAll(availabilityTask, siteTask);

        var sites = siteTask.Result.ToArray();

        var capacity = availabilityTask.Result.SelectMany(
            availability => availability.Sessions.Select(
                s => new SiteSessionInstance(sites.Single(x => x.Id == availability.Site), availability.Date.ToDateTime(s.From), availability.Date.ToDateTime(s.Until))
                {
                    Services = s.Services,
                    SlotLength = s.SlotLength,
                    Capacity = s.Capacity
                })).SelectMany(slot => slot.ToSiteSlots()).ToList();

        Console.WriteLine($"Preparing to parse {capacity.Count} report to rows - time: {timeProvider.GetUtcNow():HH:mm:ss}");

        var rows = capacity.Select(
                x => new SiteSessionParquet()
                {
                    DATE = CapacityDataConverter.ExtractDate(x),
                    TIME = CapacityDataConverter.ExtractTime(x),
                    ODS_CODE = CapacityDataConverter.ExtractOdsCode(x),
                    LATITUDE = CapacityDataConverter.ExtractLatitude(x),
                    LONGITUDE = CapacityDataConverter.ExtractLongitude(x),
                    CAPACITY = CapacityDataConverter.ExtractCapacity(x),
                    SITE_NAME = CapacityDataConverter.ExtractSiteName(x),
                    REGION = CapacityDataConverter.ExtractRegion(x),
                    ICB = CapacityDataConverter.ExtractICB(x),
                    SERVICE = CapacityDataConverter.ExtractService(x),
                }).ToList();

        Console.WriteLine($"Preparing to write {rows.Count} capacity records to {outputFile.FullName} - time: {timeProvider.GetUtcNow():HH:mm:ss}");

        using (Stream fs = outputFile.OpenWrite())
        {
            await ParquetSerializer.SerializeAsync(rows, fs);
        }

        Console.WriteLine("done");
    }
}
