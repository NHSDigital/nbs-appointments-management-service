using System;

namespace Nhs.Appointments.Core.Bookings;

public static class BookingAvailabilityStateInstructionFactory
{
    public static BookingAvailabilityStateInstruction CreateWeekSummaryInstruction(DateOnly from)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            from.ToDateTime(new TimeOnly(0, 0)), // TODO: Formalise this.
            from.AddDays(6).ToDateTime(new TimeOnly(23, 59, 59)), // TODO: Formalise this.
            BookingAvailabilityStateReturnType.Summary
            );
    }

    public static BookingAvailabilityStateInstruction CreateDaySummaryInstruction(DateOnly day)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            day.ToDateTime(new TimeOnly(0, 0)), // TODO: Formalise this.
            day.ToDateTime(new TimeOnly(23, 59, 59)), // TODO: Formalise this.
            BookingAvailabilityStateReturnType.Summary
            );
    }

    public static BookingAvailabilityStateInstruction CreateRecalculationsInstruction(DateTime from, DateTime to)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            from,
            to,
            BookingAvailabilityStateReturnType.Recalculations
            );
    }

    public static BookingAvailabilityStateInstruction CreateSessionUpdateProposalInstruction(DateTime from, DateTime to)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            from,
            to,
            BookingAvailabilityStateReturnType.SessionUpdateProposalMetrics
            );
    }

    public static BookingAvailabilityStateInstruction CreateAvailableSlotsInstruction(DateTime from, DateTime to)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            from,
            to,
            BookingAvailabilityStateReturnType.AvailableSlots
            );
    }

    public static BookingAvailabilityStateInstruction CreateDateRangeSummaryInstruction(DateOnly from, DateOnly to)
    {
        // TODO: If appropriate, create a specific implementation of an interface or base class here.
        return new BookingAvailabilityStateInstruction(
            from.ToDateTime(TimeOnly.MinValue),
            to.ToDateTime(new TimeOnly(23, 59, 59)), // TODO: Formalise this.
            BookingAvailabilityStateReturnType.Summary
            );
    }
}
