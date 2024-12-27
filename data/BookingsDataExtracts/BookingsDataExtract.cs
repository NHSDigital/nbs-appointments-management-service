using BookingsDataExtracts.Documents;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace BookingsDataExtracts;

public class BookingDataExtract(
    CosmosStore<BookingDocument> bookingsStore, 
    CosmosStore<SiteDocument> sitesStore)
{
    public async Task RunAsync()
    {
        Console.WriteLine("Loading bookings");

        var allBookings = await bookingsStore.RunQueryAsync<BookingDocument>(b => b.DocumentType == "booking" && b.From > DateTime.Today && b.From < DateTime.Today.AddMonths(3), b => b);
        var bookings = allBookings.Where(b => b.Status != AppointmentStatus.Provisional).ToList();

        Console.WriteLine("Loading sites");
        var sites = await sitesStore.RunQueryAsync<SiteDocument>(s => s.DocumentType == "site", s => s);

        var schema = new ParquetSchema(
            new DataField<string>("ODS_CODE"),
            new DataField<string>("NHS_NUMBER"),
            new DataField<string>("APPOINTMENT_DATE_TIME"),
            new DataField<string>("APPOINTMENT_STATUS"),
            new DataField<bool>("SELF_REFFERAL"),
            new DataField<string>("SOURCE"),
            new DataField<string>("DATE_OF_BIRTH"),
            new DataField<string>("BOOKING_REFERENCE_NUMBER"),
            new DataField<string>("SERVICE"),
            new DataField<string>("CREATED_DATE_TIME"),
            new DataField<double>("LATITUDE"),
            new DataField<double>("LONGITUDE"),
            new DataField<string>("SITE_NAME"),
            new DataField<string>("REGION"),
            new DataField<string>("ICB"));

        var dataConverter = new BookingDataConverter(sites);

        var odsColumn = new DataColumn(schema.DataFields[0], bookings.Select(BookingDataConverter.ExtractOdsCode).ToArray());
        var nhsNumberColumn = new DataColumn(schema.DataFields[1], bookings.Select(BookingDataConverter.ExtractNhsNumber).ToArray());
        var appointmentDateTimeColumn = new DataColumn(schema.DataFields[2], bookings.Select(BookingDataConverter.ExtractAppointmentDateTime).ToArray());
        var statusColumn = new DataColumn(schema.DataFields[3], bookings.Select(BookingDataConverter.ExtractAppointmentStatus).ToArray());
        var selfReferralColumn = new DataColumn(schema.DataFields[4], bookings.Select(BookingDataConverter.ExtractSelfReferral).ToArray());
        var sourceColumn = new DataColumn(schema.DataFields[5], bookings.Select(BookingDataConverter.ExtractSource).ToArray());
        var dateOfBirthColumn = new DataColumn(schema.DataFields[6], bookings.Select(BookingDataConverter.ExtractDateOfBirth).ToArray());
        var bookingRefColumn = new DataColumn(schema.DataFields[7], bookings.Select(BookingDataConverter.ExtractBookingReference).ToArray());
        var serviceColumn = new DataColumn(schema.DataFields[8], bookings.Select(BookingDataConverter.ExtractService).ToArray());
        var createdDateTimeColumn = new DataColumn(schema.DataFields[9], bookings.Select(BookingDataConverter.ExtractCreatedDateTime).ToArray());
        var latitudeColumn = new DataColumn(schema.DataFields[10], bookings.Select(dataConverter.ExtractLatitude).ToArray());
        var longitudeColumn = new DataColumn(schema.DataFields[11], bookings.Select(dataConverter.ExtractLongitude).ToArray());
        var siteNameColumn = new DataColumn(schema.DataFields[12], bookings.Select(dataConverter.ExtractSiteName).ToArray());
        var regionColumn = new DataColumn(schema.DataFields[13], bookings.Select(dataConverter.ExtractRegion).ToArray());
        var icbColumn = new DataColumn(schema.DataFields[14], bookings.Select(dataConverter.ExtractICB).ToArray());

        Console.WriteLine("Preparing to write");

        using (Stream fs = File.OpenWrite("bookings.parquet"))
        {
            using (ParquetWriter writer = await ParquetWriter.CreateAsync(schema, fs))
            {
                using (ParquetRowGroupWriter groupWriter = writer.CreateRowGroup())
                {

                    await groupWriter.WriteColumnAsync(odsColumn);
                    await groupWriter.WriteColumnAsync(nhsNumberColumn);
                    await groupWriter.WriteColumnAsync(appointmentDateTimeColumn);
                    await groupWriter.WriteColumnAsync(statusColumn);
                    await groupWriter.WriteColumnAsync(selfReferralColumn);
                    await groupWriter.WriteColumnAsync(sourceColumn);
                    await groupWriter.WriteColumnAsync(dateOfBirthColumn);
                    await groupWriter.WriteColumnAsync(bookingRefColumn);
                    await groupWriter.WriteColumnAsync(serviceColumn);
                    await groupWriter.WriteColumnAsync(createdDateTimeColumn);
                    await groupWriter.WriteColumnAsync(latitudeColumn);
                    await groupWriter.WriteColumnAsync(longitudeColumn);
                    await groupWriter.WriteColumnAsync(siteNameColumn);
                    await groupWriter.WriteColumnAsync(regionColumn);
                    await groupWriter.WriteColumnAsync(icbColumn);
                }
            }
        }

        Console.WriteLine("done");

    }
}
