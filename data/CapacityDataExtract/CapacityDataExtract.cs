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
                && b.Date <= DateOnly.FromDateTime(timeProvider.GetUtcNow().Date.AddDays(70)),
            b => b
        );

        var siteTask = sitesStore.RunQueryAsync(s => s.DocumentType == "site", s => s);
        
        await Task.WhenAll(availabilityTask, siteTask);

        var capacity = availabilityTask.Result.SelectMany(
            availability => availability.Sessions.Select(
                s => new SiteSessionInstance(availability.Site, availability.Date.ToDateTime(s.From), availability.Date.ToDateTime(s.Until))
                {
                    Services = s.Services,
                    SlotLength = s.SlotLength,
                    Capacity = s.Capacity
                })).SelectMany(slot => slot.ToSiteSlots()).ToList();

        var dataConverter = new CapacityDataConverter(siteTask.Result);

        Console.WriteLine("Preparing to write");

        using (Stream fs = outputFile.OpenWrite())
        {
            await ParquetSerializer.SerializeAsync(capacity.Select(
                x => new SiteSessionParquet()
                {
                    Date = CapacityDataConverter.ExtractDate(x),
                    Time = CapacityDataConverter.ExtractTime(x),
                    SlotLength = CapacityDataConverter.ExtractSlotLength(x),
                    OdsCode = dataConverter.ExtractOdsCode(x),
                    Latitude = dataConverter.ExtractLatitude(x),
                    Longitude = dataConverter.ExtractLongitude(x),
                    Capacity = CapacityDataConverter.ExtractCapacity(x),
                    SiteName = dataConverter.ExtractSiteName(x),
                    Region = dataConverter.ExtractRegion(x),
                    IntegratedCareBoard = dataConverter.ExtractICB(x),
                    Service = CapacityDataConverter.ExtractService(x),
                }), fs);
        }

        Console.WriteLine("done");
    }
}
