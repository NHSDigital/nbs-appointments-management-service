using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Core.Bookings;

public static class RecalculationExtensions
{
    public static void AppendNewlySupportedBooking(this List<BookingAvailabilityUpdate> recalculations, Booking booking)
    {
        if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
        {
            recalculations.Add(
                new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToSupported));
        }
    }

    public static void AppendNewlyUnsupportedBookings(this List<BookingAvailabilityUpdate> recalculations,
        Booking booking, NewlyUnsupportedBookingAction newlyUnsupportedBookingAction)
    {
        if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
            booking.Status is AppointmentStatus.Booked)
        {
            switch (newlyUnsupportedBookingAction)
            {
                case NewlyUnsupportedBookingAction.Orphan:
                    recalculations.Add(
                        new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToOrphaned));
                    break;
                case NewlyUnsupportedBookingAction.Cancel:
                    recalculations.Add(
                        new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.SetToCancelled));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newlyUnsupportedBookingAction), newlyUnsupportedBookingAction, null);
            }
        }
    }

    public static void AppendProvisionalBookingsToBeDeleted(this List<BookingAvailabilityUpdate> recalculations,
        Booking booking)
    {
        if (booking.Status is AppointmentStatus.Provisional)
        {
            recalculations.Add(new BookingAvailabilityUpdate(booking, AvailabilityUpdateAction.ProvisionalToDelete));
        }
    }

    public static LinkedSessionInstance FindMatchingSession(this List<LinkedSessionInstance> sessions, Session matcher)
    {
        return sessions.FirstOrDefault(s =>
                        s.From.TimeOfDay == matcher.From.ToTimeSpan() &&
                        s.Until.TimeOfDay == matcher.Until.ToTimeSpan() &&
                        s.Duration == (matcher.Until - matcher.From) &&
                        s.Capacity == matcher.Capacity &&
                        matcher.Services.All(ms => s.Services.Contains(ms))
                    );
    }
}
