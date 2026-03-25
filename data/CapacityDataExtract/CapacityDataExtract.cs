using CapacityDataExtracts.Documents;
using DataExtract;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Persistance.Models;
using Parquet.Serialization;

namespace CapacityDataExtracts;

public class CapacityDataExtract(
    CosmosStore<DailyAvailabilityDocument> availabilityStore,
    CosmosStore<SiteDocument> sitesStore,
    TimeProvider timeProvider,
    ILogger<CapacityDataExtract> logger) : IExtractor
{
    private const int ChunkSize = 10000;
    
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
                })).SelectMany(slot => slot.ToSiteSlots());

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
                });

        logger.LogInformation($"Preparing to write capacity records to {outputFile.FullName}");

        using (Stream fs = outputFile.OpenWrite())
        {
            await ParquetSerializer.SerializeAsync(rows, fs, new ParquetSerializerOptions()
            {
                RowGroupSize = ChunkSize
            });
        }

        logger.LogInformation("done");
    }
}
