using System;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;

namespace Nhs.Appointments.Api.Models;

public record EditSessionRequest(
    string Site,
    DateOnly From,
    DateOnly To,
    SessionOrWildcard SessionMatcher,
    Session? SessionReplacement,
    NewlyUnsupportedBookingAction NewlyUnsupportedBookingAction = NewlyUnsupportedBookingAction.Orphan
) : BaseSessionRequest(Site, From, To, SessionMatcher, SessionReplacement);
