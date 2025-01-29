using BookingsDataExtracts.Documents;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace BookingsDataExtracts;

public class BookingDataConverter(IEnumerable<SiteDocument> sites)
{
    public string ExtractICB(BookingDocument bookingDocument) => sites.Single(s => s.Id == bookingDocument.Site).IntegratedCareBoard;

    public string ExtractRegion(BookingDocument bookingDocument) => sites.Single(s => s.Id == bookingDocument.Site).Region;

    public string ExtractSiteName(BookingDocument bookingDocument) => sites.Single(s => s.Id == bookingDocument.Site).Name;

    public double ExtractLongitude(BookingDocument bookingDocument) => sites.Single(s => s.Id == bookingDocument.Site).Location.Coordinates[0];

    public double ExtractLatitude(BookingDocument bookingDocument) => sites.Single(s => s.Id == bookingDocument.Site).Location.Coordinates[1];

    public string ExtractOdsCode(BookingDocument booking) => sites.Single(s => s.Id == booking.Site).OdsCode;

    public static string ExtractNhsNumber(BookingDocument booking) => booking.AttendeeDetails.NhsNumber;

    public static string ExtractAppointmentDateTime(BookingDocument booking) => booking.From.ToString("yyyy-MM-dd HH:mm:ss");

    public static string ExtractCreatedDateTime(BookingDocument booking) => booking.Created.ToString("yyyy-MM-dd HH:mm:ss");

    public static string ExtractAppointmentStatus(BookingDocument booking) => booking.Status switch
    {        
        AppointmentStatus.Orphaned => "B",
        AppointmentStatus.Booked => "B",
        AppointmentStatus.Cancelled => "C",
        _ => throw new ArgumentOutOfRangeException(nameof(booking.Status)),
    };

    public static bool ExtractSelfReferral(NbsBookingDocument booking) => booking.AdditionalData?.ReferralType == "SelfReferred";

    public static string ExtractSource(NbsBookingDocument booking) => booking.AdditionalData != null ? booking.AdditionalData.Source : "Unknown";

    public static string ExtractDateOfBirth(BookingDocument booking) => booking.AttendeeDetails.DateOfBirth.ToString("yyyy-MM-dd");

    public static string ExtractBookingReference(BookingDocument booking) => booking.Reference;

    public static string ExtractService(BookingDocument booking) => booking.Service;

    public static string ExtractCancelledDateTime(BookingDocument booking) => booking.Status == AppointmentStatus.Cancelled ? booking.StatusUpdated.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
}
