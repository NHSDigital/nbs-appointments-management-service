using System;

namespace Nhs.Appointments.Api.Models
{
    public record GetDaySummaryRequest(string Site, string From)
    {
        public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
    }
}
