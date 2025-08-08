using System;

namespace Nhs.Appointments.Api.Models;

public record GetSitesByAreaRequest(
    double Longitude,
    double Latitude,
    int SearchRadius,
    int MaximumRecords,
    string[] AccessNeeds,
    bool IgnoreCache,
    string[] Services,
    string From,
    string Until)
{
    public DateOnly? FromDate
    {
        get
        {
            if(string.IsNullOrEmpty(From))
            {
                return null;
            }

            return DateOnly.ParseExact(From, "yyyy-MM-dd");
        }
    }

    public DateOnly? UntilDate
    {
        get
        {
            if(string.IsNullOrEmpty(Until))
            {
                return null;
            }

            return DateOnly.ParseExact(Until, "yyyy-MM-dd");
        }
    }
}
