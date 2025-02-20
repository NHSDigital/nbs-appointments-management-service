using CapacityDataExtract.Documents;
using DataExtract;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Parquet;
using Parquet.Schema;

namespace CapacityDataExtract;

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
                })).SelectMany(slot => slot.ToSiteSlots());

        var dataConverter = new CapacityDataConverter(siteTask.Result);

        var dataFactories = new List<DataFactory>
        {
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.Date, CapacityDataConverter.ExtractDate),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.Time, CapacityDataConverter.ExtractTime),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.SlotLength, CapacityDataConverter.ExtractSlotLength),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.OdsCode, dataConverter.ExtractOdsCode),
            new DataFactory<SiteSessionInstance, double>(CapacityDataExtractFields.Latitude, dataConverter.ExtractLatitude),
            new DataFactory<SiteSessionInstance, double>(CapacityDataExtractFields.Longitude, dataConverter.ExtractLongitude),
            new DataFactory<SiteSessionInstance, int>(CapacityDataExtractFields.Capacity, CapacityDataConverter.ExtractCapacity),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.SiteName, dataConverter.ExtractSiteName),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.Region, dataConverter.ExtractRegion),
            new DataFactory<SiteSessionInstance, string>(CapacityDataExtractFields.IntegratedCareBoard, dataConverter.ExtractICB),
            new DataFactory<SiteSessionInstance, string[]>(CapacityDataExtractFields.Service, CapacityDataConverter.ExtractService),
        };

        Console.WriteLine("Preparing to write");

        var schema = new ParquetSchema(dataFactories.Select(df => df.Field).ToArray());
        using (Stream fs = outputFile.OpenWrite())
        {
            using (var writer = await ParquetWriter.CreateAsync(schema, fs))
            {
                using (var groupWriter = writer.CreateRowGroup())
                {
                    foreach (var dataFactory in dataFactories)
                        await groupWriter.WriteColumnAsync(dataFactory.CreateColumn(capacity));
                }
            }
        }

        Console.WriteLine("done");
    }
}
