using DataExtract.Documents;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace JobRunner.Job.BookingExtract;

public class BookingDataConverter()
{
    public static string ExtractAppointmentDateTime(BookingDocument booking) => booking.From.ToString("yyyy-MM-ddTHH:mm:sszzz");

    public static string ExtractAppointmentStatus(BookingDocument booking) => booking.Status switch
    {
        AppointmentStatus.Booked => "B",
        AppointmentStatus.Cancelled => "C",
        _ => throw new ArgumentOutOfRangeException(nameof(booking.Status)),
    };

    public static bool ExtractSelfReferral(NbsBookingDocument booking) => booking.AdditionalData?.ReferralType == "SelfReferred";

    public static string ExtractDateOfBirth(BookingDocument booking) => booking.AttendeeDetails.DateOfBirth.ToString("yyyy-MM-dd");

    public static string ExtractBookingReference(BookingDocument booking) => booking.Reference;

    public static string ExtractService(BookingDocument booking) => booking.Service;
}
