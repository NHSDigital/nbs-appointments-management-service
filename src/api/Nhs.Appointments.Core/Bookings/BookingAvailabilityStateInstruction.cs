namespace Nhs.Appointments.Core.Bookings;

public record BookingAvailabilityStateInstruction(DateTime from, DateTime to, BookingAvailabilityStateReturnType returnType)
{
    public DateTime From { get; } = from;

    public DateTime To { get; } = to;

    public BookingAvailabilityStateReturnType ReturnType { get; } = returnType;
}
