using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

/// <summary>
///     Implements the comparison logic of the filter and the booking document.
///     Ideally this would live on the filter itself, but the interface lives in Core
///     and the store lives in Persistance, so there's no way of referencing the BookingDocument
///     from within the filter without a major refactor.
/// </summary>
public static class BookingQueryFilterExtensions
{
    public static bool IsMatchedBy(this BookingDocument booking, BookingQueryFilter filter)
    {
        if (filter == null || booking == null)
        {
            return false;
        }

        var siteMatch = booking.Site == filter.Site;
        var dateRangeMatch = booking.From >= filter.StartsAtOrAfter &&
                             booking.From <= filter.StartsAtOrBefore;

        var statusMatch = filter.Statuses?.Contains(booking.Status) ?? true;
        var cancellationReasonMatch = filter.CancellationReason == null ||
                                      booking.CancellationReason == filter.CancellationReason;
        var notificationStatusMatch = MatchCancellationNotificationStatus(booking, filter);

        return siteMatch && dateRangeMatch && statusMatch && cancellationReasonMatch && notificationStatusMatch;
    }

    /// <summary>
    ///     Temp oveeride: TODO: remove once Cosmos query works properly
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static bool IsMatchedBy(this Booking booking, BookingQueryFilter filter)
    {
        if (filter == null || booking == null)
        {
            return false;
        }

        var siteMatch = booking.Site == filter.Site;
        var dateRangeMatch = booking.From >= filter.StartsAtOrAfter &&
                             booking.From <= filter.StartsAtOrBefore;

        var statusMatch = filter.Statuses?.Contains(booking.Status) ?? true;
        var cancellationReasonMatch = filter.CancellationReason == null ||
                                      booking.CancellationReason == filter.CancellationReason;
        var notificationStatusMatch = MatchCancellationNotificationStatus(booking, filter);

        return siteMatch && dateRangeMatch && statusMatch && cancellationReasonMatch && notificationStatusMatch;
    }

    /// <summary>
    ///     Temp oveeride: TODO: remove once Cosmos query works properly
    /// </summary>
    /// <param name="booking"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private static bool MatchCancellationNotificationStatus(Booking booking, BookingQueryFilter filter)
    {
        // Match any booking if not filtering by cancellationNotificationStatuses
        if (filter.CancellationNotificationStatuses is null || filter.CancellationNotificationStatuses.Length == 0)
        {
            return true;
        }

        return filter.CancellationNotificationStatuses.Any(status => booking.CancellationNotificationStatus == status);
    }

    private static bool MatchCancellationNotificationStatus(BookingDocument booking, BookingQueryFilter filter)
    {
        // Match any booking if not filtering by cancellationNotificationStatuses
        if (filter.CancellationNotificationStatuses is null || filter.CancellationNotificationStatuses.Length == 0)
        {
            return true;
        }

        return filter.CancellationNotificationStatuses.Any(status => booking.CancellationNotificationStatus == status);
    }
}
