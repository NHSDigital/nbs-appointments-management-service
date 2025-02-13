using DataExtract;
using DataExtract.Documents;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Parquet;
using Parquet.Schema;

namespace BookingsDataExtracts;

public class BookingDataExtract(
    CosmosStore<NbsBookingDocument> bookingsStore,
    CosmosStore<SiteDocument> sitesStore,
    TimeProvider timeProvider) : IExtractor
{
    public async Task RunAsync(FileInfo outputFile)
    {
        Console.WriteLine("Loading bookings");
        
        var allBookings = await bookingsStore.RunQueryAsync(
            b => b.DocumentType == "booking" 
                && b.StatusUpdated > timeProvider.GetUtcNow().Date.AddDays(-1) 
                && b.StatusUpdated < timeProvider.GetUtcNow().Date, 
            b => b
        );
        var bookings = allBookings.Where(b => b.Status != AppointmentStatus.Provisional).ToList();

        Console.WriteLine("Loading sites");
        var sites = await sitesStore.RunQueryAsync(s => s.DocumentType == "site", s => s);
        var dataConverter = new BookingDataConverter(sites);

        var dataFactories = new List<DataFactory>
        {
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.OdsCode, dataConverter.ExtractOdsCode),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.NhsNumber, BookingDataConverter.ExtractNhsNumber),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.AppointmentDateTime, BookingDataConverter.ExtractAppointmentDateTime),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.AppointmentStatus, BookingDataConverter.ExtractAppointmentStatus),
            new DataFactory<NbsBookingDocument, bool>(BookingDataExtractFields.SelfRerral, BookingDataConverter.ExtractSelfReferral),
            new DataFactory<NbsBookingDocument, string>(BookingDataExtractFields.Source, BookingDataConverter.ExtractSource),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.DateOfBirth, BookingDataConverter.ExtractDateOfBirth),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.BookingReferenceNumber, BookingDataConverter.ExtractBookingReference),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.Service, BookingDataConverter.ExtractService),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.CreatedDateTime, BookingDataConverter.ExtractCreatedDateTime),
            new DataFactory<BookingDocument, double>(BookingDataExtractFields.Latitude, dataConverter.ExtractLatitude),
            new DataFactory<BookingDocument, double>(BookingDataExtractFields.Longitude, dataConverter.ExtractLongitude),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.SiteName, dataConverter.ExtractSiteName),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.Region, dataConverter.ExtractRegion),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.IntegratedCareBoard, dataConverter.ExtractICB),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.BookingSystem, doc => "MYA"),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.CancelledDateTime, BookingDataConverter.ExtractCancelledDateTime),
            new DataFactory<BookingDocument, string?>(BookingDataExtractFields.CancellationReason, doc => null),
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
                        await groupWriter.WriteColumnAsync(dataFactory.CreateColumn(bookings));                    
                }
            }
        }

        Console.WriteLine("done");
    }    
}
