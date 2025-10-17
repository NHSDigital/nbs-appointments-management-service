using System;

namespace Nhs.Appointments.Api.Models
{
    public record GetCancelledBySiteRequest(
        string Site,
        string From,
        string Until)
    {
        public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
        public DateOnly UntilDate => DateOnly.ParseExact(Until, "yyyy-MM-dd");
    }
}
