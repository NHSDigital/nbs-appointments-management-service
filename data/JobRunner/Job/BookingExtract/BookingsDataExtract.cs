using DataExtract;
using DataExtract.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;
using Parquet;
using Parquet.Schema;

namespace JobRunner.Job.BookingExtract;

public class BookingDataExtract(
    CosmosStore<NbsBookingDocument> bookingsStore,
    IOptions<BookingQueryOptions> bookingQueryOptions,
    ILogger<BookingDataExtract> logger) : IExtractor
{
    public async Task RunAsync(FileInfo outputFile)
    {
        logger.LogInformation("Loading bookings");

        var validServices = bookingQueryOptions.Value.Services;
        
        var allBookings = await bookingsStore.RunQueryAsync(
            b => b.DocumentType == "booking" 
                 && b.From >= bookingQueryOptions.Value.From.ToDateTime(new TimeOnly(0,0))
                 && b.From <= bookingQueryOptions.Value.To.ToDateTime(new TimeOnly(0,0))
                 && validServices.Contains(b.Service)
                 && (b.AdditionalData.ReferralType == "SelfReferred" || b.AttendeeDetails.DateOfBirth > new DateOnly(1950, 01,01)), 
            b => b
        );
        
        var bookings = allBookings.Where(
            b => 
                b.Status != AppointmentStatus.Provisional && 
                b.Status != AppointmentStatus.Cancelled).ToList();

        var dataFactories = new List<DataFactory>
        {
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.AppointmentDateTime, BookingDataConverter.ExtractAppointmentDateTime),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.AppointmentStatus, BookingDataConverter.ExtractAppointmentStatus),
            new DataFactory<NbsBookingDocument, bool>(BookingDataExtractFields.SelfReferral, BookingDataConverter.ExtractSelfReferral),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.DateOfBirth, BookingDataConverter.ExtractDateOfBirth),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.BookingReferenceNumber, BookingDataConverter.ExtractBookingReference),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.Service, BookingDataConverter.ExtractService),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.FirstName, document => document.AttendeeDetails.FirstName),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.PhoneNumber, document => document.ContactDetails.SingleOrDefault(x => x.Type.Equals(ContactItemType.Phone))?.Value ?? string.Empty),
            new DataFactory<BookingDocument, string>(BookingDataExtractFields.Email, document => document.ContactDetails.SingleOrDefault(x => x.Type.Equals(ContactItemType.Email))?.Value ?? string.Empty),
        };
           
        logger.LogInformation("Preparing to write");

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

        logger.LogInformation("done");
    }    
}
