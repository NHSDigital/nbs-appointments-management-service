using System;

namespace Nhs.Appointments.Api.Models;

public record CancelDateRangeRequest(
    string Site,
    DateOnly From,
    DateOnly To,
    bool CancelBookings);
