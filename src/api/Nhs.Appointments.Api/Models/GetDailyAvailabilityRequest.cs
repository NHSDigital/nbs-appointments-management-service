using System;

namespace Nhs.Appointments.Api.Models
{
    public record GetDailyAvailabilityRequest(
        string Site,
        string From,
        string To)
    {
        public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
        public DateOnly ToDate => DateOnly.ParseExact(To, "yyyy-MM-dd");
    }
}
