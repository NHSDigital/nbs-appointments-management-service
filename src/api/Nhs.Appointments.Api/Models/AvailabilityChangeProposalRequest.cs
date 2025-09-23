using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Models;

public record AvailabilityChangeProposalRequest(
    string Site, 
    string From, 
    string To, 
    Session SessionMatcher, 
    Session SessionReplacement
){
    public DateTime FromDate => DateTime.ParseExact(From, "yyyy-MM-dd", null).Date;
    public DateTime ToDate => DateTime.ParseExact(To, "yyyy-MM-dd", null)
        .Date.AddHours(23).AddMinutes(59).AddSeconds(59);
}
