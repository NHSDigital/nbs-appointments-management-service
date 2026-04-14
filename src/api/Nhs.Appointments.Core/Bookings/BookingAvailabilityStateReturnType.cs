namespace Nhs.Appointments.Core.Bookings;

public enum BookingAvailabilityStateReturnType
{
    /// <summary>
    ///     Return a list of available slots
    /// </summary>
    AvailableSlots = 0,

    /// <summary>
    ///     Return a list of bookings that need an update
    /// </summary>
    Recalculations = 1,

    /// <summary>
    ///     Return a summary of booking/availability for a period
    /// </summary>
    Summary = 2,

    /// <summary>
    ///     Return metrics that summarise the predicted outcome after a session update
    /// </summary>
    SessionUpdateProposalMetrics = 3
}
